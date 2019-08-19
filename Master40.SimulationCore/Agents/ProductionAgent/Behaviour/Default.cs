using Akka.Actor;
using Master40.DB.DataModel;
using Master40.DB.Enums;
using Master40.SimulationCore.Agents.DirectoryAgent;
using Master40.SimulationCore.Agents.HubAgent;
using Master40.SimulationCore.Helper;
using Master40.SimulationCore.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using Master40.SimulationCore.Agents.ProductionAgent.Types;
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
        internal DispoArticleDictionary _dispoArticleDictionary { get; set; } = new DispoArticleDictionary();
        internal Queue<FArticle> _childArticles { get; set; } = new Queue<FArticle>();
        internal ForwardScheduleTimeCalculator _forwardScheduleTimeCalculator { get; set; }
        public override bool Action(object message)
        {
            switch (message)
            {
                case Production.Instruction.StartProduction msg: StartProductionAgent(fArticle: msg.GetObjectFromMessage); break;
                case BasicInstruction.ResponseFromDirectory msg: SetHubAgent(hub: msg.GetObjectFromMessage); break;
                case BasicInstruction.JobForwardEnd msg: AddForwardTime(earliestStartForForwardScheduling: msg.GetObjectFromMessage); break;
                case BasicInstruction.ProvideArticle msg: ArticleProvided(msg.GetObjectFromMessage); break;
                // case Production.Instruction.FinishWorkItem fw: FinishWorkItem((Production)agent, fw.GetObjectFromMessage); break;
                // case Production.Instruction.ProductionStarted ps: ProductionStarted((Production)agent, ps.GetObjectFromMessage); break;
                // case Production.Instruction.ProvideRequest pr: ProvideRequest((Production)agent, pr.GetObjectFromMessage); break;
                // case Production.Instruction.Finished f:
                //     agent.VirtualChilds.Remove(agent.Sender);
                //     ((Production)agent).TryToFinish(); break;

                //Testing
                default: return true;
            }

            return true;
        }

        private void StartProductionAgent(FArticle fArticle)
        {
            _forwardScheduleTimeCalculator = new ForwardScheduleTimeCalculator(fArticle: fArticle);
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

            // Create Dispo Agent for each Child.
            foreach (var article in fArticle.Article.ArticleBoms)
            {
                _childArticles.Enqueue(item: article.ToRequestItem(requestItem: fArticle, requester: Agent.Context.Self, currentTime: Agent.CurrentTime));

                // create Dispo Agents for to provide required articles
                var agentSetup = AgentSetup.Create(agent: Agent, behaviour: DispoAgent.Behaviour.Factory.Get(simType: SimulationType.None));
                var instruction = Guardian.Instruction.CreateChild.Create(setup: agentSetup, target: ((IAgent)Agent).Guardian, source: Agent.Context.Self);
                Agent.Send(instruction: instruction);
            }
        }

        private void SetHubAgent(FAgentInformation hub)
        {
            // Enqueue my Element at Hub Agent
            Agent.DebugMessage(msg: $"Received Agent from Directory: {Agent.Sender.Path.Name}");

            // add agent to current Scope.
            _hubAgents.Add(key: hub.Ref, value: hub.RequiredFor);
            // foreach fitting operation
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

        /// <summary>
        /// set each material to provided and set the start condition true if all materials are provided
        /// </summary>
        /// <param name="operations"></param>
        private void ArticleProvided(FArticle fArticle)
        {
            //check vs _childArticles and if all Child article are provided set Articles to provided
            if (!fArticle.IsProvided)
                throw new Exception("Returned Article IsProvided = true has never been set.");

            Agent.DebugMessage(msg: $"Article {fArticle.Article.Name} {fArticle.Key} has been provided");
            _dispoArticleDictionary.Update(dispoRef: Agent.Sender, fArticle: fArticle);

            if(_dispoArticleDictionary.AllProvided())
            {
                Agent.DebugMessage(msg:$"All Article have been provided");

                foreach (var operation in _operationList)
                {
                    operation.StartConditions.ArticlesProvided = true;
                }
            }

        }

        private void FinishOperation(Agent agent, FOperationResult operation)
        {

        }

        internal void SetOperationReady(Agent agent)
        {

        }

        private void SendOperationStatusMsg(Agent agent, KeyValuePair<IActorRef, string> hubAgent, FOperation nextItem)
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
                    , firstOperation: (operationCounter == numberOfOperations)
                    , currentTime: Agent.CurrentTime);

                Agent.DebugMessage(
                    msg:
                    $"Created operation: {operation.Name} | BackwardStart {fJob.BackwardStart} | BackwardEnd:{fJob.BackwardEnd} Key: {fJob.Key}  ArticleKey: {fArticle.Key}");
                lastDue = fJob.BackwardStart - operation.AverageTransitionDuration;
                _operationList.Add(item: fJob);

                // send update to collector
                var pub = new FCreateSimulationWork(operation: fJob
                    , customerOrderId: fArticle.CustomerOrderId.ToString()
                    , isHeadDemand: fArticle.IsHeadDemand
                    , articleType: fArticle.Article.ArticleType.Name);
                Agent.Context.System.EventStream.Publish(@event: pub);
            }

            _fArticle = fArticle;
            SetForwardScheduling();
        }

        private void AddForwardTime(long earliestStartForForwardScheduling)
        {
            _forwardScheduleTimeCalculator.Add(earliestStartForForwardScheduling: earliestStartForForwardScheduling);
            SetForwardScheduling();
        }


        private void SetForwardScheduling()
        {
            if (!_forwardScheduleTimeCalculator.AllRequirementsFullFilled(fArticle: _fArticle))
                return;

            var operationList = new List<FOperation>();
            var earliestStart = Agent.CurrentTime;
            if (Agent.VirtualChildren.Count > 0)
                earliestStart = _forwardScheduleTimeCalculator.Max;

            foreach (var operation in _operationList.OrderBy(keySelector: x => x.Operation.HierarchyNumber))
            {
                var newOperation = operation.SetForwardSchedule(earliestStart: earliestStart);
                earliestStart = newOperation.ForwardEnd + newOperation.Operation.AverageTransitionDuration;
                operationList.Add(item: newOperation);
            }

            Agent.DebugMessage(
                msg:
                $"EarliestForwardStart {earliestStart} for Article {_fArticle.Article.Name} ArticleKey: {_fArticle.Key} send to {Agent.VirtualParent} ");

        _operationList = operationList;
            Agent.Send(instruction: BasicInstruction.JobForwardEnd.Create(message: earliestStart,
                target: Agent.VirtualParent));
        }
    }
}
