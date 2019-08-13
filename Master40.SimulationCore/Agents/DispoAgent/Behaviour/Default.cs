using System;
using System.Linq;
using Akka.Actor;
using Master40.DB.DataModel;
using Master40.DB.Enums;
using Master40.SimulationCore.Agents.ContractAgent;
using Master40.SimulationCore.Agents.DirectoryAgent;
using static Master40.SimulationCore.Agents.Guardian.Instruction;
using Master40.SimulationCore.Agents.ProductionAgent;
using Master40.SimulationCore.Agents.StorageAgent;
using Master40.SimulationCore.Agents.SupervisorAgent;
using Master40.SimulationCore.Helper;
using static FArticles;
using static FHubInformations;
using static FResourceTypes;
using static FStockReservations;

namespace Master40.SimulationCore.Agents.DispoAgent.Behaviour
{
    public class Default : SimulationCore.Types.Behaviour
    {
        internal Default(SimulationType simulationType = SimulationType.None) 
                        : base(null, simulationType) { }


        internal FArticle fArticle { get; set; }
        internal int quantityToProduce { get; set; }
        internal IActorRef storageAgentReference { get; set; }

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

        internal void RequestArticle(Dispo agent, FArticle requestItem)
        {
            var hubInformation = new FHubInformation(FResourceType.Dispo, requestItem.Article.Name, ActorRefs.Nobody);
            // get Related Storage Agent
            agent.Send(Directory.Instruction
                                     .RequestRessourceAgent
                                     .Create(descriminator: (string)requestItem.Article.Name
                                            , target: agent.ActorPaths.StorageDirectory.Ref));
            // Save Request Item.
            fArticle = requestItem;
        }

        internal void ResponseFromStock(Dispo agent, FStockReservation reservation)
        {
            var requestItem = fArticle.UpdateStockExchangeId(reservation.TrackingId);
            if (reservation == null)
            {
                throw new InvalidCastException("Could not Cast Stockreservation on Item.");
            }

            quantityToProduce = requestItem.Quantity - reservation.Quantity;
            // TODO -> Logic
            agent.DebugMessage(("Returned with " + quantityToProduce + " " + requestItem.Article.Name + " Reserved!"));

            // check If is In Stock
            if (reservation.IsInStock == true)
            {
                requestItem.SetProvided.UpdateFinishedAt(agent.CurrentTime);
                fArticle = requestItem;
                if (requestItem.IsHeadDemand)
                {
                    agent.Send(Contract.Instruction
                                       .Finish.Create(requestItem
                                                    , agent.VirtualParent
                                                    , false));
                    agent.TryToFinish();
                }
                else
                {
                    agent.Send(Production.Instruction
                                         .ProvideRequest
                                         .Create(message: fArticle.Key
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

                    agent.Send(instruction: Storage.Instruction.ProvideArticleAtDue.Create(requestItem.Key, storageAgentReference)
                                 , waitFor: requestItem.DueTime - agent.CurrentTime);
                }
            }
            fArticle = requestItem;
            // Not in Stock and Not ToBuild Agent has to Wait for Stock To Provide Materials
        }

        internal void ResponseFromHub(Dispo agent, FHubInformation hubInfo)
        {

            storageAgentReference = hubInfo.Ref;
            // debug
            agent.DebugMessage("Aquired stock Agent: " + hubInfo.Ref.Path.Name + " from " + agent.Sender.Path.Name);
            fArticle = fArticle.UpdateStorageAgent(hubInfo.Ref);
            // Create Request 
            agent.Send(Storage.Instruction.RequestArticle.Create(fArticle, hubInfo.Ref));

        }

        internal void ResponseFromSystemForBom(Dispo agent, M_Article article)
        {
            // Update
            long dueTime = fArticle.DueTime;

            if (article.WorkSchedules != null)
                dueTime = fArticle.DueTime - article.WorkSchedules.Sum(x => x.Duration); //- Calculations.GetTransitionTimeForWorkSchedules(item.Article.WorkSchedules);


            fArticle = fArticle.UpdateCustomerOrderAndDue(fArticle.CustomerOrderId, dueTime, storageAgentReference)
                                             .UpdateArticle(article);

            // Creates a Production Agent for each element that has to be produced
            for (int i = 0; i < quantityToProduce; i++)
            {
                var agentSetup = AgentSetup.Create(agent, ProductionAgent.Behaviour.Factory.Get(SimulationType.None));
                var instruction = CreateChild.Create(agentSetup, agent.Guardian);
                agent.Send(instruction);
            }
        }

        internal void RequestProvided(Dispo agent, FArticle requestItem)
        {
            agent.DebugMessage("Request Provided from " + agent.Sender);
            requestItem = requestItem.SetProvided.UpdateFinishedAt(agent.CurrentTime);

            fArticle = requestItem;

            if (requestItem.IsHeadDemand)
            {
                agent.Send(Contract.Instruction.Finish.Create(requestItem, agent.VirtualParent, false));
            }
            else
            {
                agent.Send(Production.Instruction
                                     .ProvideRequest
                                     .Create(message: fArticle.Key
                                            , target: agent.VirtualParent));
            }
        }

        internal void WithdrawMaterial(Dispo agent)
        {
            agent.Send(Storage.Instruction
                              .WithdrawlMaterial
                              .Create(message: fArticle.StockExchangeId
                                     , target: storageAgentReference));
            agent.TryToFinish();
        }
    }
}
