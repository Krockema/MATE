using Akka.Actor;
using AkkaSim.Interfaces;
using Master40.SimulationCore.MessageTypes;
using Master40.SimulationImmutables;
using System;
using System.Collections.Generic;
using static Master40.SimulationCore.Agents.DispoAgent.Instruction;

namespace Master40.SimulationCore.Agents
{
    public static class DispoBehaviour
    {

        public static BehaviourSet Default()
        {
            var actions = new Dictionary<Type, Action<Agent, ISimulationMessage>>();
            var properties = new Dictionary<string, object>();

            actions.Add(typeof(RequestArticle), RequestArticle);
            actions.Add(typeof(ResponseFromStock), ResponseFromStock);

            return new BehaviourSet(actions);
        }


        public static Action<Agent, ISimulationMessage> RequestArticle = (agent, item) =>
        {
            var requestItem = item.Message as RequestItem;
            var hubInformation = new HubInformation(ResourceType.Dispo, requestItem.Article.Name, ActorRefs.Nobody);
            // get Related Storage Agent
            agent.Send(DirectoryAgent.Instruction
                                     .RequestRessourceAgent
                                     .Create(descriminator: requestItem.Article.Name
                                            , target: agent.ActorPaths.StorageDirectory.Ref));
            // Save Request Item.
            agent.ValueStore.Add(ContractAgent.RequestItem, requestItem);
        };


        public static Action<Agent, ISimulationMessage> ResponseFromStock = (agent, item) =>
        {
            StockReservation stockReservation = item.Message as StockReservation;
            var quantityToProduce = agent.Get<decimal>(DispoAgent.QuantityToProduce);
            var requestItem = agent.Get<RequestItem>(DispoAgent.RequestItem);

            if (stockReservation == null)
            {
                throw new InvalidCastException("Could not Cast Stockreservation on Item.");
            }

            var _quantityToProduce = requestItem.Quantity - stockReservation.Quantity;
            // TODO -> Logic
            agent.DebugMessage("Returned with " + _quantityToProduce + " " + requestItem.Article.Name + " Reserved!");

            // check If is In Stock
            if (stockReservation.IsInStock == true)
            {
                requestItem = requestItem.SetProvided;
                agent.Send(ProductionAgent.Instruction
                                .ProvideRequest.Create(message: new ItemStatus(requestItem.Key, ElementStatus.Finished, 1)
                                                , target: agent.Context.Parent));
                ((DispoAgent)agent).ShutdownAgent();
                return;
            }

            // else Create Production Agents if ToBuild
            if (requestItem.Article.ToBuild)
            {
                agent.Send(SystemAgent.Instruction.RequestArticleBom.Create(requestItem, agent.ActorPaths.SystemAgent.Ref));

                // and request the Article from  stock at Due Time
                if (requestItem.IsHeadDemand)
                {
                    var stockAgent = agent.Get<IActorRef>(DispoAgent.StockAgentRef);
                    agent.Send(instruction: StorageAgent.Instruction.ProvideArticleAtDue.Create(requestItem, stockAgent)
                                       , waitFor: requestItem.DueTime - agent.CurrentTime);
                }
            }
            // Not in Stock and Not ToBuild Agent has to Wait for Stock To Provide Materials
        };
    }
}
