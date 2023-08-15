using System;
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
using static Mate.Production.Core.Helper.Negate;
using static Mate.Production.Core.Agents.BasicInstruction;
using Mate.Production.Core.Environment.Records.Interfaces;
using Mate.Production.Core.Environment.Records;
using Mate.Production.Core.Environment.Records.Reporting;
using System.Collections.Immutable;

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
        internal ArticleRecord _articleToProduce { get; set; }
        /// <summary>
        /// Class to supervise operations, supervise operation material handling, articles required by operation, and their relation
        /// </summary>
        internal OperationManager OperationManager { get; set; } = new OperationManager();
        
        internal ForwardScheduleTimeCalculator _forwardScheduleTimeCalculator { get; set; }
        public override bool Action(object message)
        {
            switch (message)
            {
                case Production.Instruction.StartProduction msg: StartProductionAgent(articleRec: msg.GetObjectFromMessage); break;
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
                nextOperation.SetStartConditions(true, nextOperation.StartCondition.ArticlesProvided, Agent.CurrentTime);
                Agent.DebugMessage(msg: $"PreCondition for operation {nextOperation.Operation.Name} {nextOperation.Key} at {_articleToProduce.Article.Name} was set true.");
                Agent.Send(instruction: BasicInstruction.UpdateStartConditions.Create(message: nextOperation.GetStartCondition(nextOperation.CustomerDue)
                                                                                      ,target: nextOperation.HubAgent));
                return;
            }

            _articleToProduce = _articleToProduce.SetProvided();
            Agent.DebugMessage(msg: $"All operations for article {_articleToProduce.Article.Name} {_articleToProduce.Key} finished.");
           

            //TODO add production amount to productionresult, currently static 1
            var productionResponse = new ProductionResultRecord(Key: _articleToProduce.Key
                                                  , TrackingId: _articleToProduce.StockExchangeId
                                                , CreationTime: _articleToProduce.CreationTime
                                                 , CustomerDue: _articleToProduce.CustomerDue
                                               , ProductionRef: Agent.Context.Self
                                                       ,Amount: 1);

            Agent.Send(instruction: Storage.Instruction.Default.ResponseFromProduction.Create(message: productionResponse, target: _articleToProduce.StorageAgent));

            if (_articleToProduce.IsHeadDemand)
            {
               
                var pub = new ThroughPutTimeRecord(ArticleKey: _articleToProduce.Key
                                            , ArticleName: _articleToProduce.Article.Name
                                            , Start: _articleToProduce.CreationTime
                                            , End: Agent.CurrentTime);
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


        private void StartProductionAgent(ArticleRecord articleRec)
        {

            _forwardScheduleTimeCalculator = new ForwardScheduleTimeCalculator(articleRec: articleRec);
            // check for Children
            if (articleRec.Article.ArticleBoms.Any())
            {
                Agent.DebugMessage(
                    msg: "Article: " + articleRec.Article.Name + " (" + articleRec.Key + ") is last leave in BOM.", CustomLogger.SCHEDULING, LogLevel.Warn);
            }

            if (articleRec.Article.Operations == null)
                throw new Exception("Production agent without operations");
            

            // Ask the Directory Agent for Service
            RequestHubAgentsFromDirectoryFor(agent: Agent, operations: articleRec.Article.Operations);
            // And create Operations
            CreateJobsFromArticle(articleRec: articleRec);

            var requiredDispoAgents = OperationManager.CreateRequiredArticles(articleToProduce: articleRec
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

        private void SetHubAgent(AgentInformationRecord hub)
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
        private void ArticleProvided(ArticleProviderRecord fArticleProvider)
        {
            var articleDictionary = OperationManager.SetArticleProvided(fArticleProvider: fArticleProvider, providedBy: Agent.Sender, currentTime: Agent.CurrentTime);
            
            Agent.DebugMessage(msg: $"Article {fArticleProvider.ArticleName} {fArticleProvider.ArticleKey} for {_articleToProduce.Article.Name} {_articleToProduce.Key} has been provided");

            var cpy = _articleToProduce.ProviderList.ToArray().ToList();
            cpy.AddRange(fArticleProvider.Provider);
            _articleToProduce = _articleToProduce.UpdateProviderList(cpy.ToImmutableHashSet());
            
            if(articleDictionary.AllProvided())
            {
                Agent.DebugMessage(msg:$"All Article for {_articleToProduce.Article.Name} {_articleToProduce.Key} have been provided");

                articleDictionary.Operation.SetStartConditions(articleDictionary.Operation.StartCondition.PreCondition, true, Agent.CurrentTime);
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

        internal void CreateJobsFromArticle(ArticleRecord articleRec)
        {
            var lastDue = articleRec.DueTime;
            var sumOperationDurations = articleRec.RemainingDuration;
            var numberOfOperations = articleRec.Article.Operations.Count();
            var operationCounter = 0;

            foreach (var operation in articleRec.Article.Operations.OrderByDescending(keySelector: x => x.HierarchyNumber))
            {
                operationCounter++;
                var fJob = operation.ToOperationRecord(dueTime: lastDue
                    , priorityRule: Agent.Configuration.GetOption<Environment.Options.PriorityRule>().Value
                    , customerDue: articleRec.CustomerDue
                    , productionAgent: Agent.Context.Self
                    , firstOperation: (operationCounter == numberOfOperations)
                    , currentTime: Agent.CurrentTime
                    , remainingWork: sumOperationDurations
                    , articleKey: articleRec.Key);

                Agent.DebugMessage(
                    msg:
                    $"Origin {articleRec.Article.Name} CustomerDue: {articleRec.CustomerDue} remainingDuration: {articleRec.RemainingDuration} Created operation: {operation.Name} | Prio {fJob.Priority.Invoke(Agent.CurrentTime)} | Remaining Work {sumOperationDurations} " +
                    $"| BackwardStart {fJob.BackwardStart} | BackwardEnd:{fJob.BackwardEnd} Key: {fJob.Key}  ArticleKey: {articleRec.Key}" + 
                    $"Precondition test: {operation.Name} | {fJob.StartCondition.PreCondition} ? {operationCounter} == {numberOfOperations} " +
                    $"| Key: {fJob.Key}  ArticleKey: {articleRec.Key}", CustomLogger.SCHEDULING, LogLevel.Warn);
                sumOperationDurations += operation.Duration;
                lastDue = fJob.BackwardStart - operation.AverageTransitionDuration;
                OperationManager.AddOperation(fJob);

                // send update to collector
                var pub = MessageFactory.ToSimulationJob(fJob
                        , jobType: JobType.OPERATION
                        , fArticle: articleRec
                        , productionAgent: this.Agent.Name
                    );
                Agent.Context.System.EventStream.Publish(@event: pub);
            }

            _articleToProduce = articleRec;
            SetForwardScheduling();
        }

        private void AddForwardTime(DateTime earliestStartForForwardScheduling)
        {

            _forwardScheduleTimeCalculator.Add(earliestStartForForwardScheduling: earliestStartForForwardScheduling);
            SetForwardScheduling();
        }

        private void UpdateCustomerDueTimes(DateTime customerDue)
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
            if (!_forwardScheduleTimeCalculator.AllRequirementsFullFilled(articleRec: _articleToProduce))
                return;

            var operationList = new List<OperationRecord>();
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
