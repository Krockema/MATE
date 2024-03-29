﻿using System;
using System.Collections.Generic;
using System.Linq;
using Akka.Actor;
using Mate.DataCore.DataModel;
using Mate.DataCore.Nominal;
using Mate.Production.Core.Agents.DirectoryAgent;
using Mate.Production.Core.Agents.DispoAgent;
using Mate.Production.Core.Agents.HubAgent;
using Mate.Production.Core.Agents.ProductionAgent.Types;
using Mate.Production.Core.Agents.StorageAgent;
using Mate.Production.Core.Helper;
using Mate.Production.Core.Types;
using NLog;
using static FAgentInformations;
using static FArticleProviders;
using static FArticles;
using static FOperations;
using static FProductionResults;
using static FThroughPutTimes;
using static IJobResults;
using static Mate.Production.Core.Helper.Negate;
using static Mate.Production.Core.Agents.BasicInstruction;

namespace Mate.Production.Core.Agents.ProductionAgent.Behaviour
{
    public class Default : Core.Types.Behaviour
    {
        internal Default(SimulationType simulationType = SimulationType.None)
            : base(childMaker: null, simulationType: simulationType)
        {
        }

        /// <summary>
        /// Operation related Hubagents
        /// </summary>
        internal AgentDictionary _hubAgents { get; set; } = new AgentDictionary();
        /// <summary>
        /// Article this Production Agent has to Produce
        /// </summary>
        internal FArticle _articleToProduce { get; set; }
        /// <summary>
        /// Class to supervise operations, supervise operation material handling, articles required by operation, and their relation
        /// </summary>
        internal OperationManager OperationManager { get; set; } = new OperationManager();
        
        internal ForwardScheduleTimeCalculator _forwardScheduleTimeCalculator { get; set; }
        public override bool Action(object message)
        {
            switch (message)
            {
                case Production.Instruction.StartProduction msg: StartProductionAgent(fArticle: msg.GetObjectFromMessage); break;
                case BasicInstruction.ResponseFromDirectory msg: SetHubAgent(hub: msg.GetObjectFromMessage); break;
                case BasicInstruction.JobForwardEnd msg: AddForwardTime(earliestStartForForwardScheduling: msg.GetObjectFromMessage); break;
                case BasicInstruction.ProvideArticle msg: ArticleProvided(msg.GetObjectFromMessage); break;
                case BasicInstruction.UpdateCustomerDueTimes msg: UpdateCustomerDueTimes(msg.GetObjectFromMessage); break;
                case BasicInstruction.WithdrawRequiredArticles msg: WithdrawRequiredArticles(operationKey: msg.GetObjectFromMessage); break;
                case BasicInstruction.FinishJob msg: FinishJob(msg.GetObjectFromMessage); break;
                case BasicInstruction.RemovedChildRef msg: TryToFinish(); break;
                case BasicInstruction.RemoveVirtualChild msg: RemoveVirtualChild(); break;
                default: return false;
            }

            return true;
        }

        private void FinishJob(IJobResult jobResult)
        {
            var nextOperation = OperationManager.GetNextOperation(key: jobResult.Key);

            if (nextOperation.IsNotNull())
            {
                nextOperation.SetStartConditions(true, nextOperation.StartConditions.ArticlesProvided, Agent.CurrentTime);
                Agent.DebugMessage(msg: $"PreCondition for operation {nextOperation.Operation.Name} {nextOperation.Key} at {_articleToProduce.Article.Name} was set true.");
                Agent.Send(instruction: BasicInstruction.UpdateStartConditions.Create(message: nextOperation.GetStartCondition(nextOperation.CustomerDue)
                                                                                      ,target: nextOperation.HubAgent));
                return;
            }

            _articleToProduce = _articleToProduce.SetProvided;
            Agent.DebugMessage(msg: $"All operations for article {_articleToProduce.Article.Name} {_articleToProduce.Key} finished.");
           

            //TODO add production amount to productionresult, currently static 1
            var productionResponse = new FProductionResult(key: _articleToProduce.Key
                                                  , trackingId: _articleToProduce.StockExchangeId
                                                , creationTime: _articleToProduce.CreationTime
                                                 , customerDue: _articleToProduce.CustomerDue
                                               , productionRef: Agent.Context.Self
                                                       ,amount: 1);

            Agent.Send(instruction: Storage.Instruction.Default.ResponseFromProduction.Create(message: productionResponse, target: _articleToProduce.StorageAgent));

            if (_articleToProduce.IsHeadDemand)
            {
               
                var pub = new FThroughPutTime(articleKey: _articleToProduce.Key
                                            , articleName: _articleToProduce.Article.Name
                                            , start: _articleToProduce.CreationTime
                                            , end: Agent.CurrentTime);
                Agent.Context.System.EventStream.Publish(@event: pub);
            }

            TryToRemoveChildRefFromProduction();

        }

        internal void RemoveVirtualChild()
        {
            Agent.VirtualChildren.Remove(Agent.Sender);
            Agent.Send(BasicInstruction.RemovedChildRef.Create(Agent.Sender));
            TryToRemoveChildRefFromProduction();
        }
        private void TryToRemoveChildRefFromProduction()
        {
            if (Agent.VirtualChildren.Count == 0 && _articleToProduce.IsProvided)
            {
                Agent.Send(BasicInstruction.RemoveVirtualChild.Create(Agent.VirtualParent));
                Agent.DebugMessage(
                    msg: $"Removing child ref from parent {Agent.VirtualParent.Path.Name} for {_articleToProduce.Article.Name} " +
                         $"(Key: {_articleToProduce.Key}, OrderId: {_articleToProduce.CustomerOrderId})"
                    , CustomLogger.DISPOPRODRELATION, LogLevel.Debug);
            }
        }
        private void TryToFinish()
        {
            if (_articleToProduce.IsProvided && Agent.VirtualChildren.Count == 0)
            {
                Agent.DebugMessage(
                    msg: $"Shutdown for {_articleToProduce.Article.Name} " +
                         $"(Key: {_articleToProduce.Key}, OrderId: {_articleToProduce.CustomerOrderId})"
                    , CustomLogger.DISPOPRODRELATION, LogLevel.Debug);
                Agent.TryToFinish();
                return;
            }

            Agent.DebugMessage(
                msg: $"Could not run shutdown for {_articleToProduce.Article.Name} " +
                     $"(Key: {_articleToProduce.Key}, OrderId: {_articleToProduce.CustomerOrderId}) cause article is provided {_articleToProduce.IsProvided } and has childs left  {Agent.VirtualChildren.Count} "
                , CustomLogger.DISPOPRODRELATION, LogLevel.Debug);
        }


        private void StartProductionAgent(FArticle fArticle)
        {

            _forwardScheduleTimeCalculator = new ForwardScheduleTimeCalculator(fArticle: fArticle);
            // check for Children
            if (fArticle.Article.ArticleBoms.Any())
            {
                Agent.DebugMessage(
                    msg: "Article: " + fArticle.Article.Name + " (" + fArticle.Key + ") is last leave in BOM.", CustomLogger.SCHEDULING, LogLevel.Warn);
            }

            if (fArticle.Article.Operations == null)
                throw new Exception("Production agent without operations");
            

            // Ask the Directory Agent for Service
            RequestHubAgentsFromDirectoryFor(agent: Agent, operations: fArticle.Article.Operations);
            // And create Operations
            CreateJobsFromArticle(fArticle: fArticle);

            var requiredDispoAgents = OperationManager.CreateRequiredArticles(articleToProduce: fArticle
                                                                                , requestingAgent: Agent.Context.Self
                                                                                , currentTime: Agent.CurrentTime);

            for (var i = 0; i < requiredDispoAgents; i++)
            {
                // create Dispo Agents for to provide required articles
                var agentSetup = AgentSetup.Create(agent: Agent,
                    behaviour: DispoAgent.Behaviour.Factory.Get(simType: SimulationType.None));
                var instruction = Guardian.Instruction.CreateChild.Create(setup: agentSetup,
                    target: ((IAgent)Agent).Guardian, source: Agent.Context.Self);
                Agent.Send(instruction: instruction);
            }
        }

        private void SetHubAgent(FAgentInformation hub)
        {
            // Enqueue my Element at Hub Agent
            Agent.DebugMessage(msg: $"Received Agent from Directory: {Agent.Sender.Path.Name}");

            // add agent to current Scope.
            _hubAgents.TryAdd(key: hub.RequiredFor, value: hub.Ref);
            // foreach fitting operation
            foreach (var operation in OperationManager.GetOperationByCapability(hub.RequiredFor))
            {
                operation.UpdateHubAgent(hub.Ref);
                Agent.Send(instruction: Hub.Instruction.Default.EnqueueJob.Create(message: operation, target: hub.Ref));
            }
        }

        /// <summary>
        /// Return from Resource over Hub to ProductionAgent to initiate to withdraw the required articles
        /// </summary>
        /// <param name="operationKey"></param>
        private void WithdrawRequiredArticles(Guid operationKey)
        {
            // Remember Only for Debugging
            var operation = OperationManager.GetOperationByKey(operationKey: operationKey);

            var dispoAgents = OperationManager.GetProviderForOperation(operationKey: operationKey);
            foreach (var dispo in dispoAgents)
            {
                Agent.DebugMessage(msg: $"Withdraw required articles for operation: {operation.Operation.Name} at {dispo.Path.Name}");
                Agent.Send(Dispo.Instruction
                                .WithdrawArticleFromStock
                                .Create(message: "Production Start"
                                    , target: dispo));
            }
            
        }

        /// <summary>
        /// set each material to provided and set the start condition true if all materials are provided
        /// </summary>
        /// <param name="fArticleProvider"></param>
        private void ArticleProvided(FArticleProvider fArticleProvider)
        {
            var articleDictionary = OperationManager.SetArticleProvided(fArticleProvider: fArticleProvider, providedBy: Agent.Sender, currentTime: Agent.CurrentTime);
            
            Agent.DebugMessage(msg: $"Article {fArticleProvider.ArticleName} {fArticleProvider.ArticleKey} for {_articleToProduce.Article.Name} {_articleToProduce.Key} has been provided");

            var cpy = _articleToProduce.ProviderList.ToArray().ToList();
            cpy.AddRange(fArticleProvider.Provider);
            _articleToProduce = _articleToProduce.UpdateProviderList(cpy);
            
            if(articleDictionary.AllProvided())
            {
                Agent.DebugMessage(msg:$"All Article for {_articleToProduce.Article.Name} {_articleToProduce.Key} have been provided");

                articleDictionary.Operation.SetStartConditions(articleDictionary.Operation.StartConditions.PreCondition, true, Agent.CurrentTime);
                if (articleDictionary.Operation.HubAgent == null) return;
                // // else 
                Agent.Send(BasicInstruction.UpdateStartConditions
                                           .Create(message: articleDictionary.Operation
                                                            .GetStartCondition(articleDictionary.Operation.CustomerDue)
                                               , target: articleDictionary.Operation.HubAgent));
            }

        }

        internal void RequestHubAgentsFromDirectoryFor(Agent agent, ICollection<M_Operation> operations)
        {
            // Request Hub Agent for Operations
            var resourceCapabilities = operations.Select(selector: x => x.ResourceCapability.Name).Distinct().ToList();
            foreach (var resourceCapabilityName in resourceCapabilities)
            {
                agent.Send(instruction: Directory.Instruction.Default
                    .RequestAgent
                    .Create(discriminator: resourceCapabilityName
                        , target: agent.ActorPaths.HubDirectory.Ref));
            }
        }

        internal void CreateJobsFromArticle(FArticle fArticle)
        {
            var lastDue = fArticle.DueTime;
            var sumOperationDurations = fArticle.RemainingDuration;
            var numberOfOperations = fArticle.Article.Operations.Count();
            var operationCounter = 0;

            foreach (var operation in fArticle.Article.Operations.OrderByDescending(keySelector: x => x.HierarchyNumber))
            {
                operationCounter++;
                var fJob = operation.ToOperationItem(dueTime: lastDue
                    , priorityRule: Agent.Configuration.GetOption<Environment.Options.PriorityRule>().Value
                    , customerDue: fArticle.CustomerDue
                    , productionAgent: Agent.Context.Self
                    , firstOperation: (operationCounter == numberOfOperations)
                    , currentTime: Agent.CurrentTime
                    , remainingWork: sumOperationDurations
                    , articleKey: fArticle.Key);

                Agent.DebugMessage(
                    msg:
                    $"Origin {fArticle.Article.Name} CustomerDue: {fArticle.CustomerDue} remainingDuration: {fArticle.RemainingDuration} Created operation: {operation.Name} | Prio {fJob.Priority.Invoke(Agent.CurrentTime)} | Remaining Work {sumOperationDurations} " +
                    $"| BackwardStart {fJob.BackwardStart} | BackwardEnd:{fJob.BackwardEnd} Key: {fJob.Key}  ArticleKey: {fArticle.Key}" + 
                    $"Precondition test: {operation.Name} | {fJob.StartConditions.PreCondition} ? {operationCounter} == {numberOfOperations} " +
                    $"| Key: {fJob.Key}  ArticleKey: {fArticle.Key}", CustomLogger.SCHEDULING, LogLevel.Warn);
                sumOperationDurations += operation.Duration;
                lastDue = fJob.BackwardStart - operation.AverageTransitionDuration;
                OperationManager.AddOperation(fJob);

                // send update to collector
                var pub = MessageFactory.ToSimulationJob(fJob
                        , jobType: JobType.OPERATION
                        , fArticle: fArticle
                        , productionAgent: this.Agent.Name
                    );
                Agent.Context.System.EventStream.Publish(@event: pub);
            }

            _articleToProduce = fArticle;
            SetForwardScheduling();
        }

        private void AddForwardTime(long earliestStartForForwardScheduling)
        {

            _forwardScheduleTimeCalculator.Add(earliestStartForForwardScheduling: earliestStartForForwardScheduling);
            SetForwardScheduling();
        }

        private void UpdateCustomerDueTimes(long customerDue)
        {

            _articleToProduce = _articleToProduce.UpdateCustomerDue(customerDue);
            foreach (var operation in OperationManager.GetOperations)
            {
                if(IsNot(operation.IsFinished))
                    Agent.Send(UpdateStartConditions
                         .Create(message: operation.GetStartCondition(customerDue)
                                , target: operation.HubAgent));
            }

            foreach (var dispoRef in Agent.VirtualChildren)
            {
                Agent.Send(BasicInstruction.UpdateCustomerDueTimes.Create(customerDue, dispoRef));
            }

        }

        private void SetForwardScheduling()
        {
            if (!_forwardScheduleTimeCalculator.AllRequirementsFullFilled(fArticle: _articleToProduce))
                return;

            var operationList = new List<FOperation>();
            var earliestStart = Agent.CurrentTime;
            if (Agent.VirtualChildren.Count > 0)
                earliestStart = _forwardScheduleTimeCalculator.Max;

            foreach (var operation in OperationManager.GetOperations.OrderBy(keySelector: x => x.Operation.HierarchyNumber))
            {
                var newOperation = operation.SetForwardSchedule(earliestStart: earliestStart);
                earliestStart = newOperation.ForwardEnd + newOperation.Operation.AverageTransitionDuration;
                operationList.Add(item: newOperation);
                Agent.DebugMessage(msg:
                    $"EarliestForwardStart| {newOperation.ForwardStart} | End: {newOperation.ForwardEnd} | Prio { newOperation.Priority.Invoke(Agent.CurrentTime)} " +
                    $"| for Operation {newOperation.Operation.Name} |Key: {newOperation.Key} | of Article {_articleToProduce.Article.Name}",
                    CustomLogger.SCHEDULING, LogLevel.Warn);

            }

            Agent.DebugMessage(msg:
                $"EarliestForwardStart {earliestStart} for Article {_articleToProduce.Article.Name} ArticleKey: {_articleToProduce.Key} send to {Agent.VirtualParent.Path.Name} ",
                CustomLogger.SCHEDULING, LogLevel.Warn);

            OperationManager.UpdateOperations(operations: operationList);
            Agent.Send(instruction: BasicInstruction.JobForwardEnd.Create(message: earliestStart,
                target: Agent.VirtualParent));
        }

        public override void OnChildAdd(IActorRef childRef)
        {
            var articleToRequest = OperationManager.Set(provider: childRef);
            Agent.Send(instruction: Dispo.Instruction.RequestArticle.Create(message: articleToRequest, target: childRef));
            Agent.DebugMessage(
                msg: $"Dispo child Agent for {articleToRequest.Article.Name} added " +
                     $"(Key: {articleToRequest.Key}, OrderId: {articleToRequest.CustomerOrderId})"
                , CustomLogger.DISPOPRODRELATION, LogLevel.Debug);
        }
    }
}
