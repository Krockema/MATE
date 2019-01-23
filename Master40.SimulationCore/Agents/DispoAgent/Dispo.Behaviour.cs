using Akka.Actor;
using Master40.DB.Models;
using Master40.SimulationCore.Helper;
using Master40.SimulationCore.MessageTypes;
using Master40.SimulationImmutables;
using System;
using System.Collections.Generic;
using System.Linq;
using static Master40.SimulationCore.Agents.Dispo;
using static Master40.SimulationCore.Agents.Dispo.Properties;

namespace Master40.SimulationCore.Agents
{
    public class DispoBehaviour : Behaviour
    {
        public DispoBehaviour(Dictionary<string, object> properties) : base(null, properties) { }



        public static DispoBehaviour Get()
        {
            var properties = new Dictionary<string, object>
            {
                { STORAGE_AGENT_REF, ActorRefs.Nobody }
                ,{ QUANTITY_TO_PRODUCE, 0.0 }
                // ,{ Dispo.Properties.REQUEST_ITEM, null }
            };

            return new DispoBehaviour(properties);
        }


        public override bool Action(Agent agent, object message)
        {
            switch (message)
            {
                case BasicInstruction.ResponseFromHub r: ResponseFromHub((Dispo)agent, r.GetObjectFromMessage); break;
                case Instruction.RequestArticle r: RequestArticle((Dispo)agent, r.GetObjectFromMessage); break;
                case Instruction.ResponseFromStock r: ResponseFromStock((Dispo)agent, r.GetObjectFromMessage);  break;
                case Instruction.ResponseFromSystemForBom r: ResponseFromSystemForBom((Dispo)agent, r.GetObjectFromMessage); break;
                case Instruction.WithdrawMaterialsFromStock r: WithdrawMaterial((Dispo)agent); break;
                case Instruction.RequestProvided r: RequestProvided((Dispo)agent, r.GetObjectFromMessage); break;
                default: return false;
            }
            return true;
        }

        private void RequestArticle(Dispo agent, RequestItem requestItem)
        {
            var hubInformation = new HubInformation(ResourceType.Dispo, requestItem.Article.Name, ActorRefs.Nobody);
            // get Related Storage Agent
            agent.Send(Directory.Instruction
                                     .RequestRessourceAgent
                                     .Create(descriminator: requestItem.Article.Name
                                            , target: agent.ActorPaths.StorageDirectory.Ref));
            // Save Request Item.
            agent.ValueStore.Add(REQUEST_ITEM, requestItem);
        }

        private void ResponseFromStock(Dispo agent, StockReservation reservation)
        {
            var quantityToProduce = agent.Get<decimal>(QUANTITY_TO_PRODUCE);
            var requestItem = agent.Get<RequestItem>(REQUEST_ITEM);

            if (reservation == null)
            {
                throw new InvalidCastException("Could not Cast Stockreservation on Item.");
            }

            var _quantityToProduce = requestItem.Quantity - reservation.Quantity;
            // TODO -> Logic
            agent.DebugMessage("Returned with " + _quantityToProduce + " " + requestItem.Article.Name + " Reserved!");

            // check If is In Stock
            if (reservation.IsInStock == true)
            {
                requestItem = requestItem.SetProvided;
                agent.Send(Production.Instruction
                                .ProvideRequest.Create(message: new ItemStatus(requestItem.Key, ElementStatus.Finished, 1)
                                                , target: agent.Context.Parent));
                agent.ShutdownAgent();
                return;
            }

            // else Create Production Agents if ToBuild
            if (requestItem.Article.ToBuild)
            {
                agent.Send(Supervisor.Instruction.RequestArticleBom.Create(requestItem, agent.ActorPaths.SystemAgent.Ref));

                // and request the Article from  stock at Due Time
                if (requestItem.IsHeadDemand)
                {
                    var stockAgent = agent.Get<IActorRef>(STORAGE_AGENT_REF);
                    agent.Send(instruction: Storage.Instruction.ProvideArticleAtDue.Create(requestItem, stockAgent)
                                       , waitFor: requestItem.DueTime - agent.CurrentTime);
                }
            }
            // Not in Stock and Not ToBuild Agent has to Wait for Stock To Provide Materials
        }

        private void ResponseFromHub(Dispo agent, HubInformation hubInfo)
        {
            var stockAgent = agent.Get<IActorRef>(STORAGE_AGENT_REF);
            var requestItem = agent.Get<RequestItem>(REQUEST_ITEM);
            if ((stockAgent = hubInfo.Ref) == null || requestItem == null)
            {
                throw new ArgumentNullException($"No storage agent found for {1}", requestItem.Article.Name);
            }
            // debug
            agent.DebugMessage("Aquired stock Agent: " + stockAgent.Path.Name + " from " + agent.Sender.Path.Name);

            // Create Request 
            agent.Send(Storage.Instruction.RequestArticle.Create(requestItem, stockAgent));
        }

        private void ResponseFromSystemForBom(Dispo agent, Article article)
        {
            // Update 
            var requestItem = agent.Get<RequestItem>(REQUEST_ITEM);
            var stockAgent = agent.Get<IActorRef>(STORAGE_AGENT_REF);
            var quantityToProduce = agent.Get<decimal>(QUANTITY_TO_PRODUCE);
            long dueTime = requestItem.DueTime;

            if (requestItem.Article.WorkSchedules != null)
                dueTime = requestItem.DueTime - requestItem.Article.WorkSchedules.Sum(x => x.Duration); //- Calculations.GetTransitionTimeForWorkSchedules(item.Article.WorkSchedules);


            RequestItem newItem = requestItem.UpdateOrderAndDue(requestItem.OrderId, dueTime, stockAgent)
                                           .UpdateArticle(article);

            // Creates a Production Agent for each element that has to be produced
            for (int i = 0; i < quantityToProduce; i++)
            {
                var agentSetup = AgentSetup.Create(agent);
                var instruction = Guardian.Instruction.CreateChild.Create(agentSetup, agent.Guardian);

                //var prodAgent = agent.Context.ActorOf(props: ProductionAgent.Props(agent.ActorPaths, agent.CurrentTime, agent.DebugThis, newItem),
                //                name: ("Production(" + newItem.Article.Name + "_Nr." + i + ")").ToActorName());
                //agent.Send(BasicInstruction.Initialize.Create(prodAgent));
            }
        }


        private void RequestProvided(Dispo agent, RequestItem requestItem)
        {
            agent.DebugMessage("Request Provided from " + agent.Sender);
            if (requestItem.IsHeadDemand)
            {
                agent.Send(Contract.Instruction.Finish.Create(requestItem, agent.Context.Parent));
            }
            else
            {
                agent.Send(Production.Instruction
                                     .ProvideRequest.Create(message: new ItemStatus(requestItem.Key, ElementStatus.Finished, 2)
                                                           , target: agent.Context.Parent));
            }
            requestItem = requestItem.SetProvided;
            agent.ShutdownAgent();
        }

        private void WithdrawMaterial(Dispo agent)
        {
            var requestItem = agent.Get<RequestItem>(REQUEST_ITEM);
            var stockAgent = agent.Get<IActorRef>(STORAGE_AGENT_REF);
            agent.Send(Storage.Instruction
                              .WithdrawlMaterial.Create(message: requestItem.StockExchangeId
                                                       , target: stockAgent));
        }
    }
}