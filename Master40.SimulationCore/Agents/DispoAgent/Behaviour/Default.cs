using System;
using System.Collections.Generic;
using System.Linq;
using Akka.Actor;
using Master40.DB.DataModel;
using Master40.DB.Enums;
using Master40.SimulationCore.Agents.ContractAgent;
using Master40.SimulationCore.Agents.DirectoryAgent;
using Master40.SimulationCore.Agents.ProductionAgent;
using Master40.SimulationCore.Agents.StorageAgent;
using Master40.SimulationCore.Agents.SupervisorAegnt;
using Master40.SimulationCore.Helper;
using Master40.SimulationImmutables;

namespace Master40.SimulationCore.Agents.DispoAgent.Behaviour
{
    public class Default : MessageTypes.Behaviour
    {
        internal Default(Dictionary<string, object> properties) : base(null, properties) { }
        public override bool Action(Agent agent, object message)
        {
            switch (message)
            {
                case BasicInstruction.ResponseFromHub r: ResponseFromHub((Dispo)agent, r.GetObjectFromMessage); break;
                case Dispo.Instruction.RequestArticle r: RequestArticle((Dispo)agent, r.GetObjectFromMessage); break;
                case Dispo.Instruction.ResponseFromStock r: ResponseFromStock((Dispo)agent, r.GetObjectFromMessage); break;
                case Dispo.Instruction.ResponseFromSystemForBom r: ResponseFromSystemForBom((Dispo)agent, r.GetObjectFromMessage); break;
                case Dispo.Instruction.WithdrawMaterialsFromStock r: WithdrawMaterial((Dispo)agent); break;
                case Dispo.Instruction.RequestProvided r: RequestProvided((Dispo)agent, r.GetObjectFromMessage); break;
                default: return false;
            }
            return true;
        }


        internal void RequestArticle(Dispo agent, FRequestItem requestItem)
        {
            var hubInformation = new FHubInformation(ResourceType.Dispo, requestItem.Article.Name, ActorRefs.Nobody);
            // get Related Storage Agent
            agent.Send(Directory.Instruction
                                     .RequestRessourceAgent
                                     .Create(descriminator: (string)requestItem.Article.Name
                                            , target: agent.ActorPaths.StorageDirectory.Ref));
            // Save Request Item.
            agent.Set(Dispo.Properties.REQUEST_ITEM, requestItem);
        }

        internal void ResponseFromStock(Dispo agent, FStockReservation reservation)
        {
            var requestItem = agent.Get<FRequestItem>(Dispo.Properties.REQUEST_ITEM);
            requestItem = requestItem.UpdateStockExchangeId(reservation.TrackingId);
            if (reservation == null)
            {
                throw new InvalidCastException("Could not Cast Stockreservation on Item.");
            }

            var quantityToProduce = requestItem.Quantity - reservation.Quantity;
            agent.Set(Dispo.Properties.QUANTITY_TO_PRODUCE, quantityToProduce);
            // TODO -> Logic
            agent.DebugMessage((string)("Returned with " + quantityToProduce + " " + requestItem.Article.Name + " Reserved!"));

            // check If is In Stock
            if (reservation.IsInStock == true)
            {
                agent.Set(Dispo.Properties.REQUEST_ITEM, requestItem.SetProvided.UpdateFinishedAt(agent.CurrentTime));
                if (requestItem.IsHeadDemand)
                {
                    agent.Send(Contract.Instruction
                                       .Finish.Create(requestItem
                                                    , agent.VirtualParent
                                                    , false));
                }
                else
                {
                    agent.Send(Production.Instruction
                                         .ProvideRequest
                                         .Create(message: new FItemStatus(requestItem.Key
                                                                        , ElementStatus.Finished, 1)
                                                , target: agent.VirtualParent));
                }
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

                    var stockAgent = agent.Get<IActorRef>(Dispo.Properties.STORAGE_AGENT_REF);
                    agent.Send(instruction: Storage.Instruction.ProvideArticleAtDue.Create(requestItem, stockAgent)
                                 , waitFor: requestItem.DueTime - agent.CurrentTime);
                }
            }
            agent.Set(Dispo.Properties.REQUEST_ITEM, requestItem);
            // Not in Stock and Not ToBuild Agent has to Wait for Stock To Provide Materials
        }

        internal void ResponseFromHub(Dispo agent, FHubInformation hubInfo)
        {
            var stockAgent = agent.Get<IActorRef>(Dispo.Properties.STORAGE_AGENT_REF);

            //    agent.Get<IActorRef>(STORAGE_AGENT_REF);
            var requestItem = agent.Get<FRequestItem>(Dispo.Properties.REQUEST_ITEM);
            agent.Set(Dispo.Properties.STORAGE_AGENT_REF, hubInfo.Ref);
            // debug
            agent.DebugMessage("Aquired stock Agent: " + hubInfo.Ref.Path.Name + " from " + agent.Sender.Path.Name);
            requestItem = requestItem.UpdateStorageAgent(hubInfo.Ref);
            agent.Set(Dispo.Properties.REQUEST_ITEM, requestItem);
            // Create Request 
            agent.Send(Storage.Instruction.RequestArticle.Create(requestItem, hubInfo.Ref));

        }

        internal void ResponseFromSystemForBom(Dispo agent, M_Article article)
        {
            // Update 
            var requestItem = agent.Get<FRequestItem>(Dispo.Properties.REQUEST_ITEM);
            var stockAgent = agent.Get<IActorRef>(Dispo.Properties.STORAGE_AGENT_REF);
            var quantityToProduce = agent.Get<int>(Dispo.Properties.QUANTITY_TO_PRODUCE);
            long dueTime = requestItem.DueTime;

            if (article.WorkSchedules != null)
                dueTime = requestItem.DueTime - article.WorkSchedules.Sum(x => x.Duration); //- Calculations.GetTransitionTimeForWorkSchedules(item.Article.WorkSchedules);


            FRequestItem newItem = requestItem.UpdateCustomerOrderAndDue(requestItem.CustomerOrderId, dueTime, stockAgent)
                                             .UpdateArticle(article);
            agent.Set(Dispo.Properties.REQUEST_ITEM, newItem);

            // Creates a Production Agent for each element that has to be produced
            for (int i = 0; i < quantityToProduce; i++)
            {
                var agentSetup = AgentSetup.Create(agent, ProductionAgent.Behaviour.BehaviourFactory.Get(SimulationType.None));
                var instruction = Guardian.Guardian.Instruction.CreateChild.Create(agentSetup, agent.Guardian);
                agent.Send(instruction);
            }
        }

        internal void RequestProvided(Dispo agent, FRequestItem requestItem)
        {
            agent.DebugMessage("Request Provided from " + agent.Sender);
            requestItem = requestItem.SetProvided.UpdateFinishedAt(agent.CurrentTime);

            agent.Set(Dispo.Properties.REQUEST_ITEM, requestItem);

            if (requestItem.IsHeadDemand)
            {
                agent.Send(Contract.Instruction.Finish.Create(requestItem, agent.VirtualParent, false));
            }
            else
            {
                agent.Send(Production.Instruction
                                     .ProvideRequest
                                     .Create(message: new FItemStatus(requestItem.Key, ElementStatus.Finished, 0)
                                            , target: agent.VirtualParent));
            }
        }

        internal void WithdrawMaterial(Dispo agent)
        {
            var requestItem = agent.Get<FRequestItem>(Dispo.Properties.REQUEST_ITEM);
            var stockAgent = agent.Get<IActorRef>(Dispo.Properties.STORAGE_AGENT_REF);
            agent.Send(Storage.Instruction
                              .WithdrawlMaterial
                              .Create(message: requestItem.StockExchangeId
                                     , target: stockAgent));
            agent.ShutdownAgent();
        }
    }
}
