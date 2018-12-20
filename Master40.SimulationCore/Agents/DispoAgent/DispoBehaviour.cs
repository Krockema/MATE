using Akka.Actor;
using AkkaSim.Interfaces;
using Master40.DB.Models;
using Master40.SimulationCore.Helper;
using Master40.SimulationCore.MessageTypes;
using Master40.SimulationImmutables;
using System;
using System.Collections.Generic;
using System.Linq;
using static Master40.SimulationCore.Agents.DispoAgent.Instruction;

namespace Master40.SimulationCore.Agents
{
    public static class DispoBehaviour
    {
        public const string STORAGE_AGENT_REF = "StorageAgentRef";
        public const string REQUEST_ITEM = "RequestItem";
        public const string QUANTITY_TO_PRODUCE = "QuantityToProduce";


        public static BehaviourSet Default()
        {
            var actions = new Dictionary<Type, Action<Agent, ISimulationMessage>>();
            var properties = new Dictionary<string, object>();

            properties.Add(STORAGE_AGENT_REF, new object { });
            //properties.Add(RequestItem, new object { });
            properties.Add(QUANTITY_TO_PRODUCE, 0.0);

            actions.Add(typeof(RequestArticle), RequestArticle);
            actions.Add(typeof(ResponseFromStock), ResponseFromStock);
            actions.Add(typeof(ResponseFromHub), ResponseFromHub);
            actions.Add(typeof(RequestProvided), RequestProvided);
            actions.Add(typeof(WithdrawMaterialsFromStock), WithdarwMaterial);

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
            agent.ValueStore.Add(REQUEST_ITEM, requestItem);
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


        public static Action<Agent, ISimulationMessage> ResponseFromHub = (agent, item) =>
        {
            var stockAgent = agent.Get<IActorRef>(STORAGE_AGENT_REF);
            var hubInfo = item.Message as HubInformation;
            var requestItem = agent.Get<RequestItem>(REQUEST_ITEM);
            if ((stockAgent = hubInfo.Ref) == null || requestItem == null)
            {
                throw new ArgumentNullException($"No storage agent found for {1}", requestItem.Article.Name);
            }
            // debug
            agent.DebugMessage("Aquired stock Agent: " + stockAgent.Path.Name + " from " + agent.Sender.Path.Name);

            // Create Request 
            agent.Send(StorageAgent.Instruction.RequestArticle.Create(requestItem, stockAgent));
        };


        public static Action<Agent, ISimulationMessage> ResponseFromSystemForBom = (agent, item) =>
        {
            // Update 
            var requestItem = agent.Get<RequestItem>(REQUEST_ITEM);
            var stockAgent = agent.Get<IActorRef>(STORAGE_AGENT_REF);
            var quantityToProduce = agent.Get<decimal>(QUANTITY_TO_PRODUCE);
            var article = item.Message as Article;
            long dueTime = requestItem.DueTime;

            if (requestItem.Article.WorkSchedules != null)
                dueTime = requestItem.DueTime - requestItem.Article.WorkSchedules.Sum(x => x.Duration); //- Calculations.GetTransitionTimeForWorkSchedules(item.Article.WorkSchedules);


            RequestItem newItem = requestItem.UpdateOrderAndDue(requestItem.OrderId, dueTime, stockAgent)
                                           .UpdateArticle(article);

            // Creates a Production Agent for each element that has to be produced
            for (int i = 0; i < quantityToProduce; i++)
            {
                var prodAgent = agent.Context.ActorOf(props: ProductionAgent.Props(agent.ActorPaths, agent.CurrentTime, agent.DebugThis, newItem),
                                name: ("Production(" + newItem.Article.Name + "_Nr." + i + ")").ToActorName());
                agent.Send(BasicInstruction.Initialize.Create(prodAgent));
            }
        };



        public static Action<Agent, ISimulationMessage> RequestProvided = (agent, item) =>
        {
            var requestItem = item.Message as RequestItem;

            agent.DebugMessage("Request Provided from " + agent.Sender);
            if (requestItem.IsHeadDemand)
            {
                agent.Send(ContractAgent.Instruction.Finish.Create(requestItem, agent.Context.Parent));
            }
            else
            {
                agent.Send(ProductionAgent.Instruction
                                                .ProvideRequest.Create(message: new ItemStatus(requestItem.Key, ElementStatus.Finished, 2)
                                                                , target: agent.Context.Parent));
            }
            requestItem = requestItem.SetProvided;
            ((DispoAgent)agent).ShutdownAgent();
        };

        public static Action<Agent, ISimulationMessage> WithdarwMaterial = (agent, item) =>
        {
            var requestItem = agent.Get<RequestItem>(REQUEST_ITEM);
            var stockAgent = agent.Get<IActorRef>(STORAGE_AGENT_REF);
            agent.Send(StorageAgent.Instruction
                                   .WithdrawlMaterial.Create(message: requestItem.StockExchangeId
                                                            , target: stockAgent));
        };
    }
}