using Akka.Actor;
using Akka.Event;
using Master40.DB.Data.Context;
using Master40.DB.Enums;
using Master40.DB.Models;
using Master40.MessageSystem.SignalR;
using Master40.SimulationCore.Helper;
using Master40.SimulationImmutables;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Master40.SimulationCore.Agents
{
    public partial class Supervisor : Agent
    {
        private ProductionDomainContext _productionDomainContext;
        private IMessageHub _messageHub;
        private SimulationConfiguration _simConfig;
        private int orderCount = 0;
        private Dictionary<int, Article> _cache = new Dictionary<int, Article>();


        // public Constructor
        public static Props Props(ActorPaths actorPaths
                                        , long time
                                        , bool debug
                                        , ProductionDomainContext productionDomainContext
                                        , IMessageHub messageHub
                                        , SimulationConfiguration simConfig
                                        , IActorRef  principal)
        {
            return Akka.Actor.Props.Create(() => new Supervisor(actorPaths, time, debug, productionDomainContext, messageHub, simConfig, principal));
        }

        public Supervisor(ActorPaths actorPaths
                                        , long time
                                        , bool debug
                                        , ProductionDomainContext productionDomainContext
                                        , IMessageHub messageHub
                                        , SimulationConfiguration simConfig
                                        , IActorRef principal) 
            : base(actorPaths, time, debug, principal)
        {
            _productionDomainContext = productionDomainContext;
            _messageHub = messageHub;
            _simConfig = simConfig;
            Send(Instruction.PopOrder.Create("Pop", ActorPaths.SystemAgent.Ref), 1);
            Send(Instruction.EndSimulation.Create(null, ActorPaths.SystemAgent.Ref), 21000);
        }

        protected override void Do(object o)
        {
            switch (o)
            {
                case Instruction.CreateContractAgent instruction: CreateContractAgent(instruction); break;
                case Instruction.RequestArticleBom instruction: RequestArticleBom(instruction.GetObjectFromMessage); break;
                case Instruction.OrderProvided instruction: OrderProvided(instruction); break;
                case Instruction.EndSimulation instruction: End(); break;
                case Instruction.PopOrder p : PopOrder(); break;
                default: throw new Exception("Invalid Message Object.");
            }
            
        }

        private void CreateContractAgent(Instruction.CreateContractAgent instruction)
        {
            var orderPart = instruction.Message as OrderPart;
            var contract = UntypedActor.Context.ActorOf(props: Contract.Props(actorPaths: ActorPaths
                                                                     , time: TimePeriod
                                                                     , debug: true)
                                           ,name: "Contract(" + orderPart.Id + ")");

            // Start Order
            Send(Contract.Instruction.StartOrder.Create(orderPart, contract));
        }

        private void RequestArticleBom(RequestItem requestItem)
        {
            //  Check 0 ref
            if (requestItem == null)
            {
                throw new InvalidCastException(this.Name + " Cast to RequestItem Failed");
            }

            // debug
            DebugMessage(" Request details for article: " + requestItem.Article.Name);

            // get BOM from Context
            _cache.TryGetValue(requestItem.Article.Id, out Article article);

            if (article == null)
            {
                article = _productionDomainContext.Articles
                                                    .Include(x => x.WorkSchedules)
                                                        .ThenInclude(x => x.MachineGroup)
                                                    .Include(x => x.ArticleBoms)
                                                        .ThenInclude(x => x.ArticleChild)
                                                    .SingleOrDefault(x => x.Id == requestItem.Article.Id);
                _cache.Add(requestItem.Article.Id, article);
            }
            // calback with po.bom
            Send(Dispo.Instruction.ResponseFromSystemForBom.Create(article, Sender));                        
        }

        private void OrderProvided(Instruction.OrderProvided instruction)
        {
            if (!(instruction.Message is RequestItem requestItem))
            {
                throw new InvalidCastException(this.Name + " Cast to RequestItem Failed");
            }

            var order = _productionDomainContext.Orders
                .Include(x => x.OrderParts)
                .Single(x => x.Id == _productionDomainContext.OrderParts.Single(s => s.Id == requestItem.OrderId).OrderId);
            order.FinishingTime = (int)this.TimePeriod;
            order.State = State.Finished;
            _productionDomainContext.SaveChanges();
            _messageHub.SendToAllClients("Oder No:" + order.Id + " finished at " + TimePeriod);
            _messageHub.ProcessingUpdate(_simConfig.Id, ++orderCount, SimulationType.Decentral, _simConfig.OrderQuantity);
        }

        private void End()
        {
            DebugMessage("End Sim");
            CoordinatedShutdown.Get(UntypedActor.Context.System).Run();
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
            foreach (var orderpart in _productionDomainContext.OrderParts
                                                                .Include(x => x.Article)
                                                                .Include(x => x.Order)
                                                                .AsNoTracking())
            {
                if (orderpart.Order.CreationTime == 0)
                {
                    Send(Instruction.CreateContractAgent.Create(orderpart, Self));
                }
                else
                {
                    long period = orderpart.Order.DueTime - (10 * 60); // 540
                    if (period < 0) { period = 0; }
                    Send(instruction: Instruction.CreateContractAgent.Create(orderpart, Self), waitFor: period);
                }
            }
            _messageHub.SendToAllClients("Agent-System ready for Work");
        }
    }
}
