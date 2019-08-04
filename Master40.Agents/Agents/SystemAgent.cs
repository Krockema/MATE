using System;
using System.Linq;
using System.Threading.Tasks;
using Master40.Agents.Agents.Internal;
using Master40.Agents.Agents.Model;
using Master40.DB.Data.Context;
using Master40.DB.Enums;
using Master40.DB.Models;
using Microsoft.EntityFrameworkCore;
using Master40.MessageSystem.SignalR;
using System.Collections.Generic;
using System.Diagnostics;

namespace Master40.Agents.Agents
{
    public class SystemAgent : Agent
    {
        private readonly ProductionDomainContext _productionDomainContext;
        private readonly IMessageHub _messageHub;
        private int orderCount = 1;
        private SimulationConfiguration _simConfig;
        private List<Dictionary<string, object>> allAgentsData = new List<Dictionary<string, object>>();

        public SystemAgent(Agent creator, string name, bool debug, ProductionDomainContext productionDomainContext, IMessageHub messageHub, SimulationConfiguration simConfig) : base(creator, name, debug)
        {
            this._productionDomainContext = productionDomainContext;
            _messageHub = messageHub;
            _simConfig = simConfig;
        }
        public enum InstuctionsMethods
        {
            ReturnData = BaseInstuctionsMethods.ReturnData,
            ReceiveData = BaseInstuctionsMethods.ReceiveData,
            CreateContractAgent,
            RequestArticleBom,
            OrderProvided,
            CollectData
        }
        //TODO: System Talk.

        private void CreateContractAgent(InstructionSet instructionSet)
        {
            //  Check 0 ref
            var requestItem = instructionSet.ObjectToProcess as OrderPart;
            if (requestItem == null)
            {
                throw new InvalidCastException(this.Name + " Cast to OrderPart Failed");
            }
            CreateAgents(requestItem);
        }

        private void RequestArticleBom(InstructionSet instructionSet)
        {
            //  Check 0 ref
            var requestItem = instructionSet.ObjectToProcess as RequestItem;
            if (requestItem == null)
            {
                throw new InvalidCastException(this.Name + " Cast to RequestItem Failed");
            }

            // debug
            DebugMessage(" Request details for article: " + requestItem.Article.Name);

            // get BOM from Context
            var article = _productionDomainContext.Articles
                                                    .Include(x => x.WorkSchedules)
                                                        .ThenInclude(x => x.MachineGroup)
                                                    .Include(x => x.ArticleBoms)
                                                        .ThenInclude(x => x.ArticleChild)
                                                    .SingleOrDefault(x => x.Id == requestItem.Article.Id);
            // calback with po.bom
            CreateAndEnqueueInstuction(methodName: DispoAgent.InstuctionsMethods.ResponseFromSystemForBom.ToString(),
                                  objectToProcess: article,
                                      targetAgent: instructionSet.SourceAgent);

        }


        private void CreateAgents(OrderPart contract)
        {
            // Create Contract agents
            var ca = new ContractAgent(creator: this,
                name: contract.Order.Name + " - Part:" + contract.Article.Name,
                debug: this.DebugThis);
            // add To System
            this.ChildAgents.Add(ca);

            // enqueue Order
            CreateAndEnqueueInstuction(methodName: ContractAgent.InstuctionsMethods.StartOrder.ToString(),
                                  objectToProcess: contract,
                                      targetAgent: ca);
                                      //, waitFor: l);

        }

        public async Task PrepareAgents(SimulationConfiguration simConfig, int simNr)
        {
            await Tools.Simulation.OrderGenerator.GenerateOrders(_productionDomainContext,simConfig, simNr);
            
            foreach (var orderpart in _productionDomainContext.OrderParts
                                                                .Include(x => x.Article)
                                                                .Include(x => x.Order)
                                                                .AsNoTracking())
            {
                if (orderpart.Order.CreationTime == 0)
                {
                    this.InstructionQueue.Enqueue(new InstructionSet
                    {
                        MethodName = SystemAgent.InstuctionsMethods.CreateContractAgent.ToString(),
                        ObjectToProcess = orderpart,
                        ObjectType = orderpart.GetType(),
                        SourceAgent = this
                    });
                }
                else
                {
                    long l = orderpart.Order.DueTime - (10*60); // 540
                    if (l < 0) { l = 0; }

                    this.CreateAndEnqueueInstuction(
                        methodName: SystemAgent.InstuctionsMethods.CreateContractAgent.ToString(),
                        objectToProcess: orderpart,
                        targetAgent: this,
                        waitFor: l
                    );
                }
            }
            _messageHub.SendToAllClients("Agent-System ready for Work");
        }

        private void OrderProvided(InstructionSet instructionSet)
        {
            if (!(instructionSet.ObjectToProcess is RequestItem requestItem))
            {
                throw new InvalidCastException(this.Name + " Cast to RequestItem Failed");
            }

            var order = _productionDomainContext.Orders
                .Include(x => x.OrderParts)
                .Single(x => x.Id == _productionDomainContext.OrderParts.Single(s => s.Id == requestItem.OrderId).OrderId);
            order.FinishingTime = (int)Context.TimePeriod;
            order.State = State.Finished;
            _productionDomainContext.SaveChanges();
            _messageHub.SendToAllClients("Oder No:" + order.Id + " finished at " + Context.TimePeriod);
            _messageHub.ProcessingUpdate(_simConfig.Id, orderCount++, SimulationType.Decentral, _simConfig.OrderQuantity);
        }

        private void CollectData(InstructionSet instructionSet)
        {
            // Gather own data
            CreateAndEnqueueInstuction(Agent.BaseInstuctionsMethods.ReturnData.ToString(), "ReturnData", this);

            //Cannot write data here since receive tasks won't be processed while this task is active 
        }

        private new void ReceiveData(InstructionSet instructionSet)
        {
            SaveChildData(instructionSet);

            if (CheckAllChildrenResponded())
            {
                DataCollectionHelper.WriteDataGrouped(allChildData);
                //allChildData = null;
            }
        }
    }
}