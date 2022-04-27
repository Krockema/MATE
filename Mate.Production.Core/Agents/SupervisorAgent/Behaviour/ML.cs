using System;
using System.Collections.Generic;
using System.Linq;
using Akka.Actor;
using AkkaSim.Definitions;
using Mate.DataCore;
using Mate.DataCore.Data.Context;
using Mate.DataCore.Data.Helper;
using Mate.DataCore.Data.Helper.Types;
using Mate.DataCore.DataModel;
using Mate.DataCore.GanttPlan;
using Mate.DataCore.Nominal;
using Mate.Production.Core.Agents.ContractAgent;
using Mate.Production.Core.Agents.DispoAgent;
using Mate.Production.Core.Agents.Guardian;
using Mate.Production.Core.Agents.SupervisorAgent.Types;
using Mate.Production.Core.Environment;
using Mate.Production.Core.Environment.Options;
using Mate.Production.Core.Helper;
using Mate.Production.Core.Helper.DistributionProvider;
using Mate.Production.Core.SignalR;
using Mate.Production.Core.Types;
using Mate_Production_AI;
using static FArticles;
using static FSetEstimatedThroughputTimes;
using static FStartOrders;
using static Mate.Production.Core.Agents.SupervisorAgent.Supervisor.Instruction;

namespace Mate.Production.Core.Agents.SupervisorAgent.Behaviour
{
    public class ML : Core.Types.Behaviour
    {
        private DataBase<MateProductionDb> dbProduction { get; set; }
        private IMessageHub _messageHub { get; set; }
        private int orderCount { get; set; } = 0;
        private long _simulationEnds { get; set; }
        private int _configID { get; set; }
        private int _settlingStart { get; set; }
        private OrderCounter _orderCounter { get; set; }
        private int _createdOrders { get; set; } = 0;
        private SimulationType _simulationType { get; set; }
        private double _transitionFactor { get; set; }
        private OrderGenerator _orderGenerator { get; set; }
        private ArticleCache _articleCache { get; set; }
        private ThroughPutDictionary _estimatedThroughPuts { get; set; } = new ThroughPutDictionary();
        private Queue<T_CustomerOrderPart> _orderQueue { get; set; } = new Queue<T_CustomerOrderPart>();
        private List<T_CustomerOrder> _openOrders { get; set; } = new List<T_CustomerOrder>();
        private ThroughputTimeAnalyzer _throughputTimeAnalyzer { get; set; } = new();
        public  ML(string dbNameProduction
            , IMessageHub messageHub
            , Configuration configuration
            , List<FSetEstimatedThroughputTimes.FSetEstimatedThroughputTime> estimatedThroughputTimes)
        {
            dbProduction = Dbms.GetMateDataBase(dbName: dbNameProduction, noTracking: false);
            _articleCache = new ArticleCache(connectionString: new DbConnectionString(dbProduction.ConnectionString.Value));
            _messageHub = messageHub;
            _orderGenerator = new OrderGenerator(simConfig: configuration, productionDomainContext: dbProduction.DbContext
                , productIds: estimatedThroughputTimes.Select(x => x.ArticleId).ToList());
            _orderCounter = new OrderCounter(maxQuantity: configuration.GetOption<OrderQuantity>().Value);
            _configID = configuration.GetOption<SimulationId>().Value;
            _settlingStart = configuration.GetOption<SettlingStart>().Value;
            _simulationEnds = configuration.GetOption<SimulationEnd>().Value;
            _simulationType = configuration.GetOption<SimulationKind>().Value;
            _transitionFactor = configuration.GetOption<TransitionFactor>().Value;
            estimatedThroughputTimes.ForEach(SetEstimatedThroughputTime);

        }
        public override bool Action(object message)
        {
            switch (message)
            {
                case BasicInstruction.ChildRef instruction: OnChildAdd(childRef: instruction.GetObjectFromMessage); break;
                // ToDo Benammung : Sollte die Letzte nachricht zwischen Produktionsagent und Contract Agent abfangen und Inital bei der ersten Forward Terminierung setzen
                case SetEstimatedThroughputTime instruction: SetEstimatedThroughputTime(getObjectFromMessage: instruction.GetObjectFromMessage); break;
                case CreateContractAgent instruction: CreateContractAgent(order: instruction.GetObjectFromMessage); break;
                case RequestArticleBom instruction: RequestArticleBom(articleId: instruction.GetObjectFromMessage); break;
                case OrderProvided instruction: OrderProvided(instruction: instruction); break;
                case SystemCheck instruction: SystemCheck(); break;
                case EndSimulation instruction: End(); break;
                case PopOrder p: PopOrder(); break;
                default: throw new Exception(message: "Invalid Message Object.");
            }

            return true;
        }

        public override bool AfterInit()
        {
            Agent.Send(instruction: Supervisor.Instruction.PopOrder.Create(message: "Pop", target: Agent.Context.Self), waitFor: 1);
            Agent.Send(instruction: EndSimulation.Create(message: true, target: Agent.Context.Self), waitFor: _simulationEnds);
            Agent.Send(instruction: Supervisor.Instruction.SystemCheck.Create(message: "CheckForOrders", target: Agent.Context.Self), waitFor: 1);
            Agent.DebugMessage(msg: "Agent-System ready for Work");
            return true;
        }

        private void SetEstimatedThroughputTime(FSetEstimatedThroughputTime getObjectFromMessage)
        {
            _estimatedThroughPuts.UpdateOrCreate(name: getObjectFromMessage.ArticleId.ToString(), time: getObjectFromMessage.Time);
        }

        private void CreateContractAgent(T_CustomerOrder order)
        {
            System.Diagnostics.Debug.WriteLine($"CreateContractAgent | {order.Id}: {Agent.CurrentTime}");
            dbProduction.DbContext.CustomerOrders.Add(order);
            dbProduction.DbContext.SaveChanges();

            var orderPart = order.CustomerOrderParts.First();
            _orderQueue.Enqueue(item: orderPart);
            Agent.DebugMessage(msg: $"Creating Contract Agent for order {order.Id} with {order.Name} DueTime {orderPart.CustomerOrder.DueTime}");
            var agentSetup = AgentSetup.Create(agent: Agent, behaviour: ContractAgent.Behaviour.Factory.Get(simType: _simulationType));
            var instruction = Instruction.CreateChild.Create(setup: agentSetup
                                               , target: Agent.ActorPaths.Guardians
                                                                  .Single(predicate: x => x.Key == GuardianType.Contract)
                                                                  .Value
                                               , source: Agent.Context.Self);

            Agent.Send(instruction: instruction);
        }

        /// <summary>
        /// After a child has been ordered from Guardian a ChildRef will be returned by the responsible child
        /// it has been allready added to this.VirtualChilds at this Point
        /// </summary>
        /// <param name="childRef"></param>
        public override void OnChildAdd(IActorRef childRef)
        {

            var order = _orderQueue.Dequeue();
            System.Diagnostics.Debug.WriteLine($"{order.CustomerOrderId}: {Agent.CurrentTime}");
            Agent.VirtualChildren.Add(item: childRef);
            Agent.Send(instruction: Contract.Instruction.StartOrder.Create(message: new FStartOrder(order, Agent.CurrentTime)
                                                        , target: childRef
                                                      , logThis: true));
        }

        private void RequestArticleBom(int articleId)
        {
            // get BOM from cached context 
            var article = _articleCache.GetArticleById(id: articleId, transitionFactor: _transitionFactor);

            Agent.DebugMessage(msg: "Request details for article: " + article.Name + " from  " + Agent.Sender.Path);

            // calback with po.bom
            Agent.Send(instruction: Dispo.Instruction.ResponseFromSystemForBom.Create(message: article, target: Agent.Sender));
        }

        private void OrderProvided(OrderProvided instruction)
        {
            if (!(instruction.Message is FArticle requestItem))
            {
                throw new InvalidCastException(message: Agent.Name + " Cast to RequestItem Failed");
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
            Agent.DebugMessage(msg: "End Sim");
            Agent.ActorPaths.SimulationContext.Ref.Tell(message: SimulationMessage.SimulationState.Finished);
        }

        private void PopOrder()
        {
            if (!_orderCounter.TryAddOne()) return;

            (var order, var nextCreation) = _orderGenerator.GetNewRandomOrder(time: Agent.CurrentTime);

            Agent.Send(instruction: Supervisor.Instruction.PopOrder.Create(message: "PopNext", target: Agent.Context.Self), waitFor: nextCreation);
            (order.TotalProcessingDuration, order.LongestPathProcessingDuration) = _throughputTimeAnalyzer.GetTimeForProduct(_articleCache, order.CustomerOrderParts.First().ArticleId, 0);

            order.ReleaseTime = order.DueTime - _estimatedThroughPuts.Get(name: order.Name).Value;

            // 1. Approach UseTransitionTimes (without prediction yet)
            order.ReleaseTime = Math.Min(order.ReleaseTime, order.DueTime - order.TotalProcessingDuration);

            // 2. Approach PredictTimeToEarly with Buffer
            var buffer = 0.25;
            var idleTime = PredictIdleTimeWithDirectRelease(order);
            order.KiReleaseTime = order.CreationTime + (int)(Convert.ToDouble(idleTime) * (1 - buffer));
            //order.ReleaseTime = Math.Min(order.DueTime - order.TotalProcessingDuration, order.KiReleaseTime);
            order.KiDueTime = order.DueTime - (int)idleTime;

            order.ReleaseTime = order.CreationTime;

            var _setDueDateByML = true;
            if (_setDueDateByML && Agent.CurrentTime > _settlingStart)
            {
                order.DueTime = Math.Max((int)order.ReleaseTime + (int)order.LongestPathProcessingDuration, order.KiDueTime);
            }

            if (Agent.CurrentTime < _settlingStart)
                order.ReleaseTime = order.CreationTime;

            System.Diagnostics.Debug.WriteLine($"EstimatedTransitionTime {order.ReleaseTime} for order {order.Name} {order.Id} , {order.DueTime}");
            Agent.DebugMessage(msg: $"EstimatedTransitionTime {order.ReleaseTime} for order {order.Name} {order.Id} , {order.DueTime}");
            
            if (order.ReleaseTime <= Agent.CurrentTime)
            {
                order.ReleaseTime = order.CreationTime;
                CreateContractAgent(order);
                return;
            }

            _openOrders.Add(item: order);
        }

        private long PredictIdleTimeWithDirectRelease(T_CustomerOrder order)
        {
            //Predicted idle time would lead to many late orders, buffer should shift the distribution
            
            var predictionData = new Predict_IdleTimeWithDirectRelease.ModelInput()
            {
                TotalProcessingDuration = order.TotalProcessingDuration,
                LongestPathProcessingDuration = order.LongestPathProcessingDuration,
                TimeToRelease = order.DueTime - order.CreationTime,
            };

            //Load model and predict output
            var result = Predict_IdleTimeWithDirectRelease.Predict(predictionData);

            return (long)(result.Score);
        }

        private void SystemCheck()
        {
            Agent.Send(instruction: Supervisor.Instruction.SystemCheck.Create(message: "CheckForOrders", target: Agent.Context.Self), waitFor: 1);

            // TODO Loop Through all CustomerOrderParts
            var orders = _openOrders.Where(x => x.ReleaseTime <= Agent.CurrentTime).ToList();
            // Debug.WriteLine("SystemCheck(" + CurrentTime + "): " + orders.Count() + " of " + _openOrders.Count() + "found");
            foreach (var order in orders)
            {
                CreateContractAgent(order);
                _openOrders.RemoveAll(match: x => x.Id == order.Id);
            }

        }
    }
}
