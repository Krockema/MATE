using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Akka.Actor;
using AkkaSim.Definitions;
using Master40.DB.Data.Context;
using Master40.DB.DataModel;
using Master40.DB.Enums;
using Master40.SimulationCore.Agents.ContractAgent;
using Master40.SimulationCore.Agents.DispoAgent;
using Master40.SimulationCore.Agents.Guardian;
using Master40.SimulationCore.Environment;
using Master40.SimulationCore.Environment.Options;
using Master40.SimulationCore.Helper;
using Master40.SimulationImmutables;
using Master40.Tools.SignalR;
using Microsoft.EntityFrameworkCore;

namespace Master40.SimulationCore.Agents.SupervisorAgent
{
    public partial class Supervisor : Agent
    {
        private ProductionDomainContext _productionDomainContext;
        private IMessageHub _messageHub;
        private int orderCount { get; set; } = 0;
        private int _configID;
        private int _orderMaxQuantity;
        private int _createdOrders { get; set; } = 0;
        private SimulationType _simulationType;
        private Dictionary<string, EstimatedThroughPut> _estimatedThroughPuts = new Dictionary<string, EstimatedThroughPut>();
        private OrderGenerator _orderGenerator;
        private Dictionary<int, M_Article> _cache = new Dictionary<int, M_Article>();
        private Queue<T_CustomerOrderPart> _orderQueue = new Queue<T_CustomerOrderPart>();
        private List<T_CustomerOrder> _openOrders = new List<T_CustomerOrder>();


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
            _messageHub = messageHub;
            _orderGenerator = new OrderGenerator(configuration, _productionDomainContext, productIds);
            _orderMaxQuantity = configuration.GetOption<OrderQuantity>().Value;
            _configID = configuration.GetOption<SimulationId>().Value;
            _simulationType = configuration.GetOption<SimulationKind>().Value;
            SetInitialThroughput(configuration.GetOption<EstimatedThroughPut>().Value);
            Send(Instruction.PopOrder.Create("Pop", Self), 1);
            Send(Instruction.EndSimulation.Create(true, Self), configuration.GetOption<SimulationEnd>().Value);
            Send(Instruction.SystemCheck.Create("CheckForOrders", Self), 1);
            DebugMessage("Agent-System ready for Work");
        }

        private void SetInitialThroughput(long eta)
        {
            var names = _productionDomainContext.ArticleBoms
                                                .Include(c => c.ArticleChild)
                                                .Where(b => b.ArticleParentId == null)
                                                .AsNoTracking()
                                                .Select(a => new { a.ArticleChild.Name, a.ArticleChild.Id })
                                                .ToList();
            foreach (var name in names)
            {
                _estimatedThroughPuts.Add(name.Name, new EstimatedThroughPut(eta));
            }
   
        }

        protected override void Do(object o)
        {
            switch (o)
            {
                case BasicInstruction.ChildRef instruction: OnChildAdd(instruction.GetObjectFromMessage); break;
                case Instruction.CreateContractAgent instruction: CreateContractAgent(instruction.GetObjectFromMessage); break;
                case Instruction.RequestArticleBom instruction: RequestArticleBom(instruction.GetObjectFromMessage); break;
                case Instruction.OrderProvided instruction: OrderProvided(instruction); break;
                case Instruction.SetEstimatedThroughputTime instruction: SetEstimatedThroughputTime(instruction.GetObjectFromMessage); break;
                case Instruction.SystemCheck instruction: SystemCheck(); break;
                case Instruction.EndSimulation instruction: End(); break;
                case Instruction.PopOrder p : PopOrder(); break;
                default: throw new Exception("Invalid Message Object.");
            }
            
        }

        private void SetEstimatedThroughputTime(FSetEstimatedThroughputTime getObjectFromMessage)
        {
            _estimatedThroughPuts.TryGetValue(getObjectFromMessage.ArticleName, out EstimatedThroughPut eta);
            eta.Set(getObjectFromMessage.Time);
        }

        private void CreateContractAgent(T_CustomerOrderPart orderPart)
        {
            _orderQueue.Enqueue(orderPart);
            DebugMessage(" Creating Contract Agent");
            var agentSetup = AgentSetup.Create(this, ContractBehaviour.Get());
            var instruction = Guardian.Guardian.Instruction
                                      .CreateChild
                                      .Create(agentSetup, ActorPaths.Guardians
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
            VirtualChilds.Add(childRef, ElementStatus.Created);
            Send(Contract.Instruction.StartOrder.Create(_orderQueue.Dequeue(), childRef, true));
        }

        private void RequestArticleBom(FRequestItem requestItem)
        {
            //  Check 0 ref
            if (requestItem == null)
            {
                throw new InvalidCastException(this.Name + " Cast to RequestItem Failed");
            }

            // debug
            DebugMessage(" Request details for article: " + requestItem.Article.Name + " from  " + Sender.Path);

            // get BOM from Context
            _cache.TryGetValue((int)requestItem.Article.Id, out M_Article article);

            if (article == null)
            {
                article = Queryable.SingleOrDefault(source: _productionDomainContext.Articles
                                                        .Include(x => x.WorkSchedules)
                                                        .ThenInclude(x => x.ResourceSkill)
                                                        .Include(x => x.ArticleBoms)
                                                            .ThenInclude(x => x.ArticleChild), 
                                                    predicate: (x => x.Id == requestItem.Article.Id));
                _cache.Add(requestItem.Article.Id, article);
            }
            // calback with po.bom
            Send(Dispo.Instruction.ResponseFromSystemForBom.Create(article, Sender));                        
        }

        private void OrderProvided(Instruction.OrderProvided instruction)
        {
            if (!(instruction.Message is FRequestItem requestItem))
            {
                throw new InvalidCastException(this.Name + " Cast to RequestItem Failed");
            }

            var order = _productionDomainContext.CustomerOrders
                .Include(x => x.CustomerOrderParts)
                .Single(x => x.Id == _productionDomainContext.CustomerOrderParts.Single(s => s.Id == requestItem.CustomerOrderId).CustomerOrderId);
            order.FinishingTime = (int)this.TimePeriod;
            order.State = State.Finished;
            _productionDomainContext.SaveChanges();
            _messageHub.ProcessingUpdate(_configID, ++orderCount, SimulationType.Decentral.ToString(), _orderMaxQuantity);
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
            if (_createdOrders >= _orderMaxQuantity)
                return;
            _createdOrders++;
            var order = _orderGenerator.GetNewRandomOrder(time: CurrentTime);
            Send(Instruction.PopOrder.Create("PopNext", Self), order.CreationTime - CurrentTime);
            _estimatedThroughPuts.TryGetValue(order.CustomerOrderParts.First().Article.Name, out EstimatedThroughPut eta);

            long period = order.DueTime - (eta.Value); // 1 Tag un 1 Schich
            if (period < 0)
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
            var orders = _openOrders.Where(x => x.DueTime - _estimatedThroughPuts
                                                    .Single(y => y.Key == x.CustomerOrderParts.First().Article.Name)
                                                        .Value.Value <= this.TimePeriod).ToList();
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
