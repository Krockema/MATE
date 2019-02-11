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

        public class ActorRef
        {
            public IActorRef Ref { get; set; }
        }


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

        private void RequestArticle(Dispo agent, FRequestItem requestItem)
        {
            var hubInformation = new FHubInformation(ResourceType.Dispo, requestItem.Article.Name, ActorRefs.Nobody);
            // get Related Storage Agent
            agent.Send(Directory.Instruction
                                     .RequestRessourceAgent
                                     .Create(descriminator: requestItem.Article.Name
                                            , target: agent.ActorPaths.StorageDirectory.Ref));
            // Save Request Item.
            agent.Set(REQUEST_ITEM, requestItem);
        }

        private void ResponseFromStock(Dispo agent, FStockReservation reservation)
        {
            var requestItem = agent.Get<FRequestItem>(REQUEST_ITEM);

            if (reservation == null)
            {
                throw new InvalidCastException("Could not Cast Stockreservation on Item.");
            }

            var quantityToProduce = requestItem.Quantity - reservation.Quantity;
            agent.Set(QUANTITY_TO_PRODUCE, quantityToProduce);
            // TODO -> Logic
            agent.DebugMessage("Returned with " + quantityToProduce + " " + requestItem.Article.Name + " Reserved!");

            // check If is In Stock
            if (reservation.IsInStock == true)
            {
                agent.Set(REQUEST_ITEM, requestItem.SetProvided);
                agent.Send(Production.Instruction
                                .ProvideRequest.Create(message: new FItemStatus(requestItem.Key, ElementStatus.Finished, 1)
                                                , target: agent.VirtualParent));
                return;
            }

            // else Create Production Agents if ToBuild
            if (requestItem.Article.ToBuild)
            {
                agent.Send(Supervisor.Instruction.RequestArticleBom.Create(requestItem, agent.ActorPaths.SystemAgent.Ref));

                // and request the Article from  stock at Due Time
                if (requestItem.IsHeadDemand)
                {
                    agent.DebugMessage("Agent Id: " + agent.Key + "Request From Stroage at due : " + (requestItem.DueTime - agent.CurrentTime));

                    var stockAgent = agent.Get<IActorRef>(STORAGE_AGENT_REF);
                    agent.Send(instruction: Storage.Instruction.ProvideArticleAtDue.Create(requestItem, stockAgent)
                                 , waitFor: requestItem.DueTime - agent.CurrentTime);
                }
            }
            // Not in Stock and Not ToBuild Agent has to Wait for Stock To Provide Materials
        }

        private void ResponseFromHub(Dispo agent, FHubInformation hubInfo)
        {
            var stockAgent = agent.Get<IActorRef>(STORAGE_AGENT_REF);
                        
            //    agent.Get<IActorRef>(STORAGE_AGENT_REF);
            var requestItem = agent.Get<FRequestItem>(REQUEST_ITEM);
            agent.Set(STORAGE_AGENT_REF, hubInfo.Ref);
            // debug
            agent.DebugMessage("Aquired stock Agent: " + hubInfo.Ref.Path.Name + " from " + agent.Sender.Path.Name);
            requestItem = requestItem.UpdateStorageAgent(hubInfo.Ref);
            agent.Set(REQUEST_ITEM, requestItem);
            // Create Request 
            agent.Send(Storage.Instruction.RequestArticle.Create(requestItem, hubInfo.Ref));
            
        }

        private void ResponseFromSystemForBom(Dispo agent, Article article)
        {
            // Update 
            var requestItem = agent.Get<FRequestItem>(REQUEST_ITEM);
            var stockAgent = agent.Get<IActorRef>(STORAGE_AGENT_REF);
            var quantityToProduce = agent.Get<int>(QUANTITY_TO_PRODUCE);
            long dueTime = requestItem.DueTime;

            if (article.WorkSchedules != null)
                dueTime = requestItem.DueTime - article.WorkSchedules.Sum(x => x.Duration); //- Calculations.GetTransitionTimeForWorkSchedules(item.Article.WorkSchedules);


            FRequestItem newItem = requestItem.UpdateOrderAndDue(requestItem.OrderId, dueTime, stockAgent)
                                             .UpdateArticle(article);
            agent.Set(REQUEST_ITEM, newItem);

            // Creates a Production Agent for each element that has to be produced
            for (int i = 0; i < quantityToProduce; i++)
            {
                var agentSetup = AgentSetup.Create(agent, ProductionBehaviour.Get());
                var instruction = Guardian.Instruction.CreateChild.Create(agentSetup, agent.Guardian);
                agent.Send(instruction);
            }
        }

        private void RequestProvided(Dispo agent, FRequestItem requestItem)
        {
            agent.DebugMessage("Request Provided from " + agent.Sender);
            requestItem = requestItem.SetProvided;
            agent.Set(REQUEST_ITEM, requestItem.SetProvided);

            if (requestItem.IsHeadDemand)
            {
                agent.Send(Contract.Instruction.Finish.Create(requestItem, agent.VirtualParent));
            }
            else
            {
                agent.Send(Production.Instruction
                                     .ProvideRequest
                                     .Create(message: new FItemStatus(requestItem.Key, ElementStatus.Finished, 0)
                                            , target: agent.VirtualParent));
            }
        }

        private void WithdrawMaterial(Dispo agent)
        {
            var requestItem = agent.Get<FRequestItem>(REQUEST_ITEM);
            var stockAgent = agent.Get<IActorRef>(STORAGE_AGENT_REF);
            agent.Send(Storage.Instruction
                              .WithdrawlMaterial
                              .Create(message: requestItem.StockExchangeId
                                                       , target: stockAgent));
            agent.ShutdownAgent();
        }
    }
}