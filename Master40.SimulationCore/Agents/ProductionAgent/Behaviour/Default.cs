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
                : base(null, simulationType) { }

        internal List<FOperation> _operationList { get; set; } = new List<FOperation>();
        internal FOperation _nextOperation { get; set; }
        internal AgentDictionary _hubAgents { get; set; } = new AgentDictionary();
        internal FArticle _fArticle { get; set; }
        internal List<FArticle> _requestedItemList { get; set; } = new List<FArticle>();
        internal Queue<FArticle> _childArticles { get; set; } = new Queue<FArticle>();

        public override bool Action(object message)
        {
            switch (message)
            {
                case Production.Instruction.StartProduction i: StartProductionAgent(i.GetObjectFromMessage); break;
                case BasicInstruction.ResponseFromDirectory s: SetHubAgent(s.GetObjectFromMessage); break;
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
            var firstToEnqueue = false;
            // check for Children
            if (fArticle.Article.ArticleBoms.Any())
            {
                Agent.DebugMessage("Article: " + fArticle.Article.Name +" (" + fArticle.Key + ") is last leave in BOM.");
                firstToEnqueue = true;
            }

            // if item has Operations request HubAgent for them
            if (fArticle.Article.Operations != null)
            {
                // Ask the Directory Agent for Service
                RequestHubAgentsFromDirectoryFor(agent: Agent, operations: fArticle.Article.Operations);
                // And create Operations
                CreateJobsFromArticle(firstItemToBuild: firstToEnqueue, fArticle: fArticle);
            }

            // Create Dispo Agents for each Child.
            foreach (var article in fArticle.Article.ArticleBoms)
            {
                _childArticles.Enqueue(article.ToRequestItem(fArticle, Agent.Context.Self, Agent.CurrentTime));

                // create Dispo Agents for to Provide Required Articles
                var agentSetup = AgentSetup.Create(Agent, DispoAgent.Behaviour.Factory.Get(SimulationType.None));
                var instruction = Guardian.Instruction.CreateChild.Create(agentSetup, Agent.Guardian);
                Agent.Send(instruction);
            }
            _fArticle = fArticle;
        }

        private void SetHubAgent(FAgentInformation hub)
        {
            // Enque my Element at Comunication Agent
            Agent.DebugMessage($"Received Agent from Directory: {Agent.Sender.Path.Name}");

            // add agent to current Scope.
            _hubAgents.Add(hub.Ref, hub.RequiredFor);
            // foreach fitting WorkSchedule
            foreach (var operation in _operationList.Where(x => x.Operation.ResourceSkill.Name == hub.RequiredFor))
            {
                Agent.Send(Hub.Instruction.EnqueueJob.Create(operation, hub.Ref));
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
            var resourceSkills = operations.Select(x => x.ResourceSkill.Name).Distinct().ToList();
            foreach (var resourceSkillName in resourceSkills)
            {
                agent.Send(Directory.Instruction
                            .RequestAgent
                            .Create(discriminator: resourceSkillName
                                    , target: agent.ActorPaths.HubDirectory.Ref));
            }
        }

        internal void CreateJobsFromArticle(bool firstItemToBuild, FArticle fArticle)
        {
            var lastDue = fArticle.DueTime;
            foreach (var operation in fArticle.Article.Operations.OrderBy(x => x.HierarchyNumber))
            {
                var fJob = operation.ToOperationItem(dueTime: lastDue
                                         ,productionAgent: Agent.Context.Self
                                                ,lastLeaf: firstItemToBuild
                                             ,currentTime: Agent.CurrentTime);

                Agent.DebugMessage("Created operation: " + operation.Name + " | Due:" + lastDue + " Key: " + fJob.Key);
                lastDue = lastDue - operation.Duration;
                firstItemToBuild = false;
                _operationList.Add(item: fJob);
                // ToDO; 
                var pub = new FCreateSimulationWork(operation: fJob
                                             ,customerOrderId: fArticle.CustomerOrderId.ToString()
                                               , isHeadDemand: fArticle.IsHeadDemand
                                                , articleType: fArticle.Article.ArticleType.Name);
                Agent.Context.System.EventStream.Publish(@event: pub);
            }

        }
    }
}
