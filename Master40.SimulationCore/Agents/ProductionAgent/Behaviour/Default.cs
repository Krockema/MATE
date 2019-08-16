﻿using Akka.Actor;
using Master40.DB.DataModel;
using Master40.DB.Enums;
using Master40.SimulationCore.Agents.DirectoryAgent;
using Master40.SimulationCore.Agents.HubAgent;
using Master40.SimulationCore.Helper;
using Master40.SimulationCore.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using static FArticles;
using static FAgentInformations;
using static FOperationResults;
using static FOperations;
using static FCreateSimulationWorks;

namespace Master40.SimulationCore.Agents.ProductionAgent.Behaviour
{
    public class Default : SimulationCore.Types.Behaviour
    {
        internal Default(SimulationType simulationType = SimulationType.None)
            : base(childMaker: null, obj: simulationType)
        {
        }

        internal List<FOperation> _operationList { get; set; } = new List<FOperation>();
        internal FOperation _nextOperation { get; set; }
        internal AgentDictionary _hubAgents { get; set; } = new AgentDictionary();
        internal FArticle _fArticle { get; set; }
        internal List<FArticle> _requestedItemList { get; set; } = new List<FArticle>();
        internal Queue<FArticle> _childArticles { get; set; } = new Queue<FArticle>();
        internal List<long> _forwardScheduledEndingTimes { get; set; } = new List<long>();
        public override bool Action(object message)
        {
            switch (message)
            {
                case Production.Instruction.StartProduction msg: StartProductionAgent(fArticle: msg.GetObjectFromMessage); break;
                case BasicInstruction.ResponseFromDirectory msg: SetHubAgent(hub: msg.GetObjectFromMessage); break;
                case BasicInstruction.JobForwardEnd msg: AddForwardTime(ealiestStartForForwardScheduling: msg.GetObjectFromMessage); break;
                // case Production.Instruction.FinishWorkItem fw: FinishWorkItem((Production)agent, fw.GetObjectFromMessage); break;
                // case Production.Instruction.ProductionStarted ps: ProductionStarted((Production)agent, ps.GetObjectFromMessage); break;
                // case Production.Instruction.ProvideRequest pr: ProvideRequest((Production)agent, pr.GetObjectFromMessage); break;
                // case Production.Instruction.Finished f:
                //     agent.VirtualChilds.Remove(agent.Sender);
                //     ((Production)agent).TryToFinish(); break;
                default: return false;
            }

            return true;
        }

        private void StartProductionAgent(FArticle fArticle)
        {
            // check for Children
            if (fArticle.Article.ArticleBoms.Any())
            {
                Agent.DebugMessage(
                    msg: "Article: " + fArticle.Article.Name + " (" + fArticle.Key + ") is last leave in BOM.");
            }

            // if item has Operations request HubAgent for them
            if (fArticle.Article.Operations != null)
            {
                // Ask the Directory Agent for Service
                RequestHubAgentsFromDirectoryFor(agent: Agent, operations: fArticle.Article.Operations);
                // And create Operations
                CreateJobsFromArticle(fArticle: fArticle);
            }

            // Create Dispo Agents for each Child.
            foreach (var article in fArticle.Article.ArticleBoms)
            {
                _childArticles.Enqueue(item: article.ToRequestItem(requestItem: fArticle, requester: Agent.Context.Self, currentTime: Agent.CurrentTime));

                // create Dispo Agents for to Provide Required Articles
                var agentSetup = AgentSetup.Create(agent: Agent, behaviour: DispoAgent.Behaviour.Factory.Get(simType: SimulationType.None));
                var instruction = Guardian.Instruction.CreateChild.Create(setup: agentSetup, target: ((Production)Agent).Guardian, source: Agent.Context.Self);
                Agent.Send(instruction: instruction);
            }

            _fArticle = fArticle;
        }

        private void SetHubAgent(FAgentInformation hub)
        {
            // Enque my Element at Comunication Agent
            Agent.DebugMessage(msg: $"Received Agent from Directory: {Agent.Sender.Path.Name}");

            // add agent to current Scope.
            _hubAgents.Add(key: hub.Ref, value: hub.RequiredFor);
            // foreach fitting WorkSchedule
            foreach (var operation in _operationList.Where(predicate: x => x.Operation.ResourceSkill.Name == hub.RequiredFor))
            {
                Agent.Send(instruction: Hub.Instruction.EnqueueJob.Create(message: operation, target: hub.Ref));
            }
        }

        private void ProductionStarted(Agent agent, Guid workItem)
        {

        }

        internal void Finished(Agent agent, FOperationResult operationResult)
        {

        }

        private void ProvideRequest(Production agent, Guid operationResult)
        {

        }

        private void SetMaterialsProvided(List<FOperation> workItems)
        {

        }

        private void FinishWorkItem(Agent agent, FOperationResult operation)
        {

        }

        internal void SetWorkItemReady(Agent agent)
        {

        }

        private void SendWorkItemStatusMsg(Agent agent, KeyValuePair<IActorRef, string> hubAgent, FOperation nextItem)
        {

        }

        internal void RequestHubAgentsFromDirectoryFor(Agent agent, ICollection<M_Operation> operations)
        {
            // Request Hub Agent for Operations
            var resourceSkills = operations.Select(selector: x => x.ResourceSkill.Name).Distinct().ToList();
            foreach (var resourceSkillName in resourceSkills)
            {
                agent.Send(instruction: Directory.Instruction
                    .RequestAgent
                    .Create(discriminator: resourceSkillName
                        , target: agent.ActorPaths.HubDirectory.Ref));
            }
        }

        internal void CreateJobsFromArticle(FArticle fArticle)
        {
            var lastDue = fArticle.DueTime;
            var numberOfOperations = fArticle.Article.Operations.Count();
            var operationCounter = 0;
            foreach (var operation in fArticle.Article.Operations.OrderByDescending(keySelector: x => x.HierarchyNumber))
            {
                numberOfOperations++;
                var fJob = operation.ToOperationItem(dueTime: lastDue
                    , productionAgent: Agent.Context.Self
                    , firstOperation: (operationCounter == numberOfOperations)?true:false
                    , currentTime: Agent.CurrentTime);

                Agent.DebugMessage(
                    msg:
                    $"Created operation: {operation.Name} | BackwardStart {fJob.BackwardStart} | BackwardEnd:{fJob.BackwardEnd} Key: {fJob.Key}");
                lastDue = fJob.BackwardStart - operation.AverageTransitionDuration;
                _operationList.Add(item: fJob);

                // send update to collector
                var pub = new FCreateSimulationWork(operation: fJob
                    , customerOrderId: fArticle.CustomerOrderId.ToString()
                    , isHeadDemand: fArticle.IsHeadDemand
                    , articleType: fArticle.Article.ArticleType.Name);
                Agent.Context.System.EventStream.Publish(@event: pub);
            }

            SetForwardScheduling();
        }

        private void AddForwardTime(long ealiestStartForForwardScheduling)
        {
            _forwardScheduledEndingTimes.Add(ealiestStartForForwardScheduling);
            SetForwardScheduling();
        }


        private void SetForwardScheduling()
        {   
            if (_forwardScheduledEndingTimes.Count != Agent.VirtualChildren.Count)
                return;

            var operationList = new List<FOperation>();
            var earliestStart = Agent.CurrentTime;
            if (Agent.VirtualChildren.Count > 0)
                earliestStart = _forwardScheduledEndingTimes.Max();
            
            foreach (var operation in _operationList.OrderBy(keySelector: x => x.Operation.HierarchyNumber))
            {
                var newOperation = operation.SetForwardSchedule(earliestStart: earliestStart);
                earliestStart = newOperation.ForwardEnd + newOperation.Operation.AverageTransitionDuration;
                operationList.Add(item: newOperation);
            }

            _operationList = operationList;
            Agent.Send(instruction: BasicInstruction.JobForwardEnd.Create(message: earliestStart, target: Agent.VirtualParent));
        }
    }
}
