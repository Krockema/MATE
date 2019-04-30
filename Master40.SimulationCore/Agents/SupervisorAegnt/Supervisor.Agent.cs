﻿using System;
using System.Collections.Generic;
using System.Linq;
using Akka.Actor;
using AkkaSim.Definitions;
using Master40.DB.Data.Context;
using Master40.DB.DataModel;
using Master40.DB.Enums;
using Master40.DB.ReportingModel;
using Master40.SimulationCore.Agents.ContractAgent;
using Master40.SimulationCore.Agents.DispoAgent;
using Master40.SimulationCore.Agents.Guardian;
using Master40.SimulationCore.Helper;
using Master40.SimulationImmutables;
using Master40.Tools.SignalR;
using Microsoft.EntityFrameworkCore;

namespace Master40.SimulationCore.Agents.SupervisorAegnt
{
    public partial class Supervisor : Agent
    {
        private ProductionDomainContext _productionDomainContext;
        private IMessageHub _messageHub;
        private SimulationConfiguration _simConfig;
        private int orderCount = 0;
        private Dictionary<int, M_Article> _cache = new Dictionary<int, M_Article>();
        private Queue<T_CustomerOrderPart> _orderQueue = new Queue<T_CustomerOrderPart>();


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
            Send(Instruction.EndSimulation.Create(true, Self), _simConfig.SimulationEndTime);
        }

        protected override void Do(object o)
        {
            switch (o)
            {
                case BasicInstruction.ChildRef instruction: OnChildAdd(instruction.GetObjectFromMessage); break;
                case Instruction.CreateContractAgent instruction: CreateContractAgent(instruction.GetObjectFromMessage); break;
                case Instruction.RequestArticleBom instruction: RequestArticleBom(instruction.GetObjectFromMessage); break;
                case Instruction.OrderProvided instruction: OrderProvided(instruction); break;
                case Instruction.EndSimulation instruction: End(); break;
                case Instruction.PopOrder p : PopOrder(); break;
                default: throw new Exception("Invalid Message Object.");
            }
            
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
                article = Queryable.SingleOrDefault(_productionDomainContext.Articles
                                                        .Include(x => x.WorkSchedules)
                                                        .ThenInclude(x => x.MachineGroup)
                                                        .Include(x => x.ArticleBoms)
                                                        .ThenInclude(x => x.ArticleChild), (System.Linq.Expressions.Expression<Func<M_Article, bool>>)(x => x.Id == requestItem.Article.Id));
                _cache.Add((int)requestItem.Article.Id, article);
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
            _messageHub.ProcessingUpdate(_simConfig.Id, ++orderCount, SimulationType.Decentral.ToString(), _simConfig.OrderQuantity);
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
            foreach (var orderpart in _productionDomainContext.CustomerOrderParts
                                                                .Include(x => x.Article)
                                                                    .ThenInclude(x => x.ArticleBoms)
                                                                        .ThenInclude(x => x.ArticleChild)
                                                                .Include(x => x.CustomerOrder)
                                                                .AsNoTracking())
            {
                if (orderpart.CustomerOrder.CreationTime == 0)
                {
                    Send(Instruction.CreateContractAgent.Create(orderpart, Self));
                }
                else
                {
                    long period = orderpart.CustomerOrder.DueTime - (_simConfig.Time); // 1 Tag un 1 Schich
                    if (period < 0) { period = 0; }
                    Send(instruction: Instruction.CreateContractAgent.Create(orderpart, Self)
                           , waitFor: period);
                }
            }
            //_messageHub.SendToAllClients("Agent-System ready for Work");
            DebugMessage("Agent-System ready for Work");
        }
    }
}
