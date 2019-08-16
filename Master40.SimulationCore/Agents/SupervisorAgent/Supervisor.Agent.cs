using Akka.Actor;
using AkkaSim.Definitions;
using Master40.DB.Data.Context;
using Master40.DB.DataModel;
using Master40.DB.Enums;
using Master40.SimulationCore.Agents.ContractAgent;
using Master40.SimulationCore.Agents.DispoAgent;
using Master40.SimulationCore.Agents.Guardian;
using static Master40.SimulationCore.Agents.Guardian.Instruction;
using Master40.SimulationCore.Agents.Types;
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
using Master40.SimulationCore.DistributionProvider;
using Master40.SimulationCore.Agents.SupervisorAgent.Types;

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
                                        , List<int> productIds
                                        , IActorRef  principal)
        {
            return Akka.Actor.Props.Create(() => new Supervisor(actorPaths, time, debug, productionDomainContext, messageHub, configuration, productIds, principal));
        }

        public Supervisor(ActorPaths actorPaths
                                        , long time
                                        , bool debug
                                        , ProductionDomainContext productionDomainContext
                                        , IMessageHub messageHub
                                        , Configuration configuration
                                        , List<int> productIds
                                        , IActorRef principal
                                        ) 
            : base(actorPaths, time, debug, principal)
        {
            _productionDomainContext = productionDomainContext;
            _dataBaseConnection = _productionDomainContext.Database.GetDbConnection();
            _articleCache = new ArticleCache(_dataBaseConnection.ConnectionString);
            _messageHub = messageHub;
            _orderGenerator = new OrderGenerator(configuration, _productionDomainContext, productIds);
            _orderCounter = new OrderCounter(configuration.GetOption<OrderQuantity>().Value);
            _configID = configuration.GetOption<SimulationId>().Value;
            _simulationType = configuration.GetOption<SimulationKind>().Value;
            _transitionFactor = configuration.GetOption<TransitionFactor>().Value;
            Send(Instruction.PopOrder.Create("Pop", Self), 1);
            Send(Instruction.EndSimulation.Create(true, Self), configuration.GetOption<SimulationEnd>().Value);
            Send(Instruction.SystemCheck.Create("CheckForOrders", Self), 1);
            DebugMessage("Agent-System ready for Work");
        }

        protected override void Do(object o)
        {
            switch (o)
            {
                case BasicInstruction.ChildRef instruction: OnChildAdd(instruction.GetObjectFromMessage); break;
                case Instruction.SetEstimatedThroughputTime instruction: SetEstimatedThroughputTime(instruction.GetObjectFromMessage); break; // ToDo Benammung : Sollte die Letzte nachricht zwischen Produktionsagent und Contract Agent abfangen und Inital bei der ersten Forward Terminierung setzen
                case Instruction.CreateContractAgent instruction: CreateContractAgent(instruction.GetObjectFromMessage); break;
                case Instruction.RequestArticleBom instruction: RequestArticleBom(instruction.GetObjectFromMessage); break;
                case Instruction.OrderProvided instruction: OrderProvided(instruction); break;
                case Instruction.SystemCheck instruction: SystemCheck(); break;
                case Instruction.EndSimulation instruction: End(); break;
                case Instruction.PopOrder p : PopOrder(); break;
                default: throw new Exception("Invalid Message Object.");
            }
            
        }

        private void SetEstimatedThroughputTime(FSetEstimatedThroughputTime getObjectFromMessage)
        {
            _estimatedThroughPuts.UpdateOrCreate(getObjectFromMessage.ArticleName, getObjectFromMessage.Time);
        }

        private void CreateContractAgent(T_CustomerOrderPart orderPart)
        {
            _orderQueue.Enqueue(orderPart);
            DebugMessage("Creating Contract Agent for order " + orderPart.CustomerOrderId);
            var agentSetup = AgentSetup.Create(this, ContractAgent.Behaviour.Factory.Get(simType: _simulationType));
            var instruction = CreateChild.Create(setup: agentSetup
                                               ,target: ActorPaths.Guardians
                                                                  .Single(x => x.Key == GuardianType.Contract)
                                                                  .Value);

            Send(instruction);
        }

        /// <summary>
        /// After a child has been ordered from Guardian a ChildRef will be returned by the responsible child
        /// it has been allready added to this.VirtualChilds at this Point
        /// </summary>
        /// <param name="childRef"></param>
        protected override void OnChildAdd(IActorRef childRef)
        {
            VirtualChildren.Add(childRef);
            Send(Contract.Instruction.StartOrder.Create(message: _orderQueue.Dequeue()
                                                        ,target: childRef
                                                      , logThis: true));
        }

        private void RequestArticleBom(int articleId)
        {
            // get BOM from cached context 
            var article = _articleCache.GetArticleById(articleId, _transitionFactor);

            DebugMessage("Request details for article: " + article.Name + " from  " + Sender.Path);

            // calback with po.bom
            Send(Dispo.Instruction.ResponseFromSystemForBom.Create(article, Sender));                        
        }

        private void OrderProvided(Instruction.OrderProvided instruction)
        {
            if (!(instruction.Message is FArticle requestItem))
            {
                throw new InvalidCastException(this.Name + " Cast to RequestItem Failed");
            }

            var order = _productionDomainContext.CustomerOrders
                .Include(x => x.CustomerOrderParts)
                .Single(x => x.Id == _productionDomainContext.CustomerOrderParts.Single(s => s.Id == requestItem.CustomerOrderId).CustomerOrderId);
            order.FinishingTime = (int)this.TimePeriod;
            order.State = State.Finished;
            _productionDomainContext.SaveChanges();
            _messageHub.ProcessingUpdate(_configID, _orderCounter.ProvidedOrder(), SimulationType.Decentral.ToString(), _orderCounter.Max);
        }

        private void End()
        {
            DebugMessage("End Sim");
            _SimulationContext.Tell(SimulationMessage.SimulationState.Finished);
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
            Send(Instruction.PopOrder.Create("PopNext", Self), order.CreationTime - CurrentTime);
            var eta = _estimatedThroughPuts.Get(order.CustomerOrderParts.First().Article.Name);

            long period = order.DueTime - (eta.Value); // 1 Tag und 1 Schicht
            if (period < 0 || eta.Value == 0)
            {
                order.CustomerOrderParts.ToList()
                     .ForEach(item => CreateContractAgent(item));
                return;
            }
            _openOrders.Add(order);
        }

        private void SystemCheck()
        {
            Send(Instruction.SystemCheck.Create("CheckForOrders", Self), 1);

            // TODO Loop Through all CustomerOrderParts
            var orders = _openOrders.Where(x => x.DueTime - _estimatedThroughPuts.Get(x.CustomerOrderParts.First().Article.Name).Value <= this.TimePeriod).ToList();
           // Debug.WriteLine("SystemCheck(" + CurrentTime + "): " + orders.Count() + " of " + _openOrders.Count() + "found");
            foreach (var order in orders)
            {
                order.CustomerOrderParts.ToList()
                     .ForEach(item => CreateContractAgent(item));
                _openOrders.RemoveAll(x => x.Id == order.Id);
            }
            
        }
    }
}
