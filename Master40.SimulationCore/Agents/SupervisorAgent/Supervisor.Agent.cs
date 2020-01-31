using Akka.Actor;
using AkkaSim.Definitions;
using Master40.DB.Data.Context;
using Master40.DB.DataModel;
using Master40.DB.Nominal;
using Master40.SimulationCore.Agents.ContractAgent;
using Master40.SimulationCore.Agents.DispoAgent;
using Master40.SimulationCore.Agents.Guardian;
using static Master40.SimulationCore.Agents.Guardian.Instruction;
using Master40.SimulationCore.Environment;
using Master40.SimulationCore.Environment.Options;
using Master40.SimulationCore.Helper;
using Master40.SimulationCore.Types;
using Master40.Tools.SignalR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using static FArticles;
using static FSetEstimatedThroughputTimes;
using Master40.SimulationCore.Agents.SupervisorAgent.Types;
using Master40.SimulationCore.Helper.DistributionProvider;

namespace Master40.SimulationCore.Agents.SupervisorAgent
{
    public partial class Supervisor : Agent
    {
        private ProductionDomainContext _productionDomainContext { get; set; }
        private DbConnection _dataBaseConnection { get; set; }
        private IMessageHub _messageHub { get; set; }
        private int orderCount { get; set; } = 0;
        private int _configID { get; set; }
        private OrderCounter _orderCounter { get; set; }
        private int _createdOrders { get; set; } = 0;
        private SimulationType _simulationType { get; set; }
        private decimal _transitionFactor { get; set; }
        private OrderGenerator _orderGenerator { get; set; }
        private ArticleCache _articleCache { get; set; }
        private ThroughPutDictionary _estimatedThroughPuts { get; set; } = new ThroughPutDictionary();
        private Queue<T_CustomerOrderPart> _orderQueue { get; set; } = new Queue<T_CustomerOrderPart>();
        private List<T_CustomerOrder> _openOrders { get; set; } = new List<T_CustomerOrder>();


        // public Constructor
        public static Props Props(ActorPaths actorPaths
                                        , long time
                                        , bool debug
                                        , ProductionDomainContext productionDomainContext
                                        , IMessageHub messageHub
                                        , Configuration configuration
                                        , List<FSetEstimatedThroughputTime> estimatedThroughputTimes
                                        , IActorRef  principal)
        {
            return Akka.Actor.Props.Create(factory: () => new Supervisor(actorPaths, time, debug, productionDomainContext, messageHub, configuration, estimatedThroughputTimes, principal));
        }

        public Supervisor(ActorPaths actorPaths
                                        , long time
                                        , bool debug
                                        , ProductionDomainContext productionDomainContext
                                        , IMessageHub messageHub
                                        , Configuration configuration
                                        , List<FSetEstimatedThroughputTime> estimatedThroughputTimes
                                        , IActorRef principal
                                        ) 
            : base(actorPaths: actorPaths, time: time, debug: debug, principal: principal)
        {
            _productionDomainContext = productionDomainContext;
            _dataBaseConnection = _productionDomainContext.Database.GetDbConnection();
            _articleCache = new ArticleCache(connectionString: _dataBaseConnection.ConnectionString);
            _messageHub = messageHub;
            _orderGenerator = new OrderGenerator(simConfig: configuration, productionDomainContext: _productionDomainContext
                                              , productIds: estimatedThroughputTimes.Select(x => x.ArticleId).ToList());
            _orderCounter = new OrderCounter(maxQuantity: configuration.GetOption<OrderQuantity>().Value);
            _configID = configuration.GetOption<SimulationId>().Value;
            _simulationType = configuration.GetOption<SimulationKind>().Value;
            _transitionFactor = configuration.GetOption<TransitionFactor>().Value;
            estimatedThroughputTimes.ForEach(SetEstimatedThroughputTime);
            Send(instruction: Instruction.PopOrder.Create(message: "Pop", target: Self), waitFor: 1);
            Send(instruction: Instruction.EndSimulation.Create(message: true, target: Self), waitFor: configuration.GetOption<SimulationEnd>().Value);
            Send(instruction: Instruction.SystemCheck.Create(message: "CheckForOrders", target: Self), waitFor: 1);
            DebugMessage(msg: "Agent-System ready for Work");
        }

        protected override void Do(object o)
        {
            switch (o)
            {
                case BasicInstruction.ChildRef instruction: OnChildAdd(childRef: instruction.GetObjectFromMessage); break;
                // ToDo Benammung : Sollte die Letzte nachricht zwischen Produktionsagent und Contract Agent abfangen und Inital bei der ersten Forward Terminierung setzen
                case Instruction.SetEstimatedThroughputTime instruction: SetEstimatedThroughputTime(getObjectFromMessage: instruction.GetObjectFromMessage); break; 
                case Instruction.CreateContractAgent instruction: CreateContractAgent(orderPart: instruction.GetObjectFromMessage); break;
                case Instruction.RequestArticleBom instruction: RequestArticleBom(articleId: instruction.GetObjectFromMessage); break;
                case Instruction.OrderProvided instruction: OrderProvided(instruction: instruction); break;
                case Instruction.SystemCheck instruction: SystemCheck(); break;
                case Instruction.EndSimulation instruction: End(); break;
                case Instruction.PopOrder p : PopOrder(); break;
                default: throw new Exception(message: "Invalid Message Object.");
            }
            
        }

        private void SetEstimatedThroughputTime(FSetEstimatedThroughputTime getObjectFromMessage)
        {
            _estimatedThroughPuts.UpdateOrCreate(name: getObjectFromMessage.ArticleName, time: getObjectFromMessage.Time);
        }

        private void CreateContractAgent(T_CustomerOrderPart orderPart)
        {
            _orderQueue.Enqueue(item: orderPart);
            DebugMessage(msg: $"Creating Contract Agent for order {orderPart.CustomerOrderId} with {orderPart.Article.Name} DueTime {orderPart.CustomerOrder.DueTime}");
            var agentSetup = AgentSetup.Create(agent: this, behaviour: ContractAgent.Behaviour.Factory.Get(simType: _simulationType));
            var instruction = CreateChild.Create(setup: agentSetup
                                               ,target: ActorPaths.Guardians
                                                                  .Single(predicate: x => x.Key == GuardianType.Contract)
                                                                  .Value
                                               , source: this.Self);

            Send(instruction: instruction);
        }

        /// <summary>
        /// After a child has been ordered from Guardian a ChildRef will be returned by the responsible child
        /// it has been allready added to this.VirtualChilds at this Point
        /// </summary>
        /// <param name="childRef"></param>
        protected override void OnChildAdd(IActorRef childRef)
        {
            VirtualChildren.Add(item: childRef);
            Send(instruction: Contract.Instruction.StartOrder.Create(message: _orderQueue.Dequeue()
                                                        ,target: childRef
                                                      , logThis: true));
        }

        private void RequestArticleBom(int articleId)
        {
            // get BOM from cached context 
            var article = _articleCache.GetArticleById(id: articleId, transitionFactor: _transitionFactor);

            DebugMessage(msg: "Request details for article: " + article.Name + " from  " + Sender.Path);

            // calback with po.bom
            Send(instruction: Dispo.Instruction.ResponseFromSystemForBom.Create(message: article, target: Sender));                        
        }

        private void OrderProvided(Instruction.OrderProvided instruction)
        {
            if (!(instruction.Message is FArticle requestItem))
            {
                throw new InvalidCastException(message: this.Name + " Cast to RequestItem Failed");
            }

            // var order = _productionDomainContext.CustomerOrders
            //     .Include(navigationPropertyPath: x => x.CustomerOrderParts)
            //     .Single(predicate: x => x.Id == _productionDomainContext.CustomerOrderParts.Single(s => s.Id == requestItem.CustomerOrderId).CustomerOrderId);
            // order.FinishingTime = (int)this.TimePeriod;
            // order.State = State.Finished;
            //_productionDomainContext.SaveChanges();
            // _messageHub.ProcessingUpdate(simId: _configID, finished: _orderCounter.ProvidedOrder(), simType: SimulationType.Decentral.ToString(), max: _orderCounter.Max);
        }

        private void End()
        {
            DebugMessage(msg: "End Sim");
            _SimulationContext.Tell(message: SimulationMessage.SimulationState.Finished);
        }

        protected override void Finish()
        {
            if (Sender == ActorPaths.SimulationContext.Ref)
            {
                base.Finish();
            }
        }

        private void PopOrder()
        {
            if (!_orderCounter.TryAddOne()) return;

            var order = _orderGenerator.GetNewRandomOrder(time: CurrentTime);
            
            Send(instruction: Instruction.PopOrder.Create(message: "PopNext", target: Self), waitFor: order.CreationTime - CurrentTime);
            var eta = _estimatedThroughPuts.Get(name: order.CustomerOrderParts.First().Article.Name);
            DebugMessage(msg: $"EstimatedTransitionTime {eta.Value} for order {order.Name} {order.Id}");

            long period = order.DueTime - (eta.Value); // 1 Tag und 1 Schicht
            if (period < 0 || eta.Value == 0)
            {
                order.CustomerOrderParts.ToList()
                     .ForEach(action: item => CreateContractAgent(orderPart: item));
                return;
            }
            _openOrders.Add(item: order);
        }

        private void SystemCheck()
        {
            Send(instruction: Instruction.SystemCheck.Create(message: "CheckForOrders", target: Self), waitFor: 1);

            // TODO Loop Through all CustomerOrderParts
            var orders = _openOrders.Where(predicate: x => x.DueTime - _estimatedThroughPuts.Get(name: x.CustomerOrderParts.First().Article.Name).Value <= this.TimePeriod).ToList();
           // Debug.WriteLine("SystemCheck(" + CurrentTime + "): " + orders.Count() + " of " + _openOrders.Count() + "found");
            foreach (var order in orders)
            {
                order.CustomerOrderParts.ToList()
                     .ForEach(action: item => CreateContractAgent(orderPart: item));
                _openOrders.RemoveAll(match: x => x.Id == order.Id);
            }
            
        }
    }
}
