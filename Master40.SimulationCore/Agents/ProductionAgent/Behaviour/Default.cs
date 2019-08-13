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
using static FHubInformations;
using static FOperationResults;
using static FOperations;

namespace Master40.SimulationCore.Agents.ProductionAgent.Behaviour
{
    public class Default : SimulationCore.Types.Behaviour
    {
        internal Default(SimulationType simulationType = SimulationType.None)
                : base(null, simulationType) { }

        internal List<FOperation> operationList { get; set; } = new List<FOperation>();
        internal FOperation nextOperation { get; set; }
        internal AgentDictionary hubAgents { get; set; } = new AgentDictionary();
        internal FArticle fArticle { get; set; }
        internal List<FArticle> requestedItemList { get; set; } = new List<FArticle>();
        internal Queue<FArticle> childOperations { get; set; } = new Queue<FArticle>();

        public override bool Action(Agent agent, object message)
        {
            switch (message)
            {
                case Production.Instruction.StartProduction i: StartProductionAgent((Production)agent, i.GetObjectFromMessage); break;
                case BasicInstruction.ResponseFromHub s: SetHubAgent((Production)agent, s.GetObjectFromMessage); break;
                case Production.Instruction.FinishWorkItem fw: FinishWorkItem((Production)agent, fw.GetObjectFromMessage); break;
                case Production.Instruction.ProductionStarted ps: ProductionStarted((Production)agent, ps.GetObjectFromMessage); break;
                case Production.Instruction.ProvideRequest pr: ProvideRequest((Production)agent, pr.GetObjectFromMessage); break;
                case Production.Instruction.Finished f:
                    agent.VirtualChilds.Remove(agent.Sender);
                    ((Production)agent).TryToFinish(); break;
                default: return false;
            }
            return true;
        }
        private void StartProductionAgent(Production agent, FArticle requestItem)
        {
            var firstToEnqueue = false;
            // check for Children
            if (Enumerable.Any(requestItem.Article.ArticleBoms))
            {
                agent.DebugMessage("Last leave in Bom");
                firstToEnqueue = true;
            }

            // if item hase Workschedules Request ComClient for them
            if (requestItem.Article.WorkSchedules != null)
            {
                // Ask the Directory Agent for Service
                RequestHubAgentFor(agent, workSchedules: fArticle.Article.WorkSchedules);
                // And Create workItems
                CreateWorkItemsFromRequestItem(firstItemToBuild: firstToEnqueue, requestItem);
            }

            // Create Dispo Agents for Childs.
            foreach (var articleBom in fArticle.Article.ArticleBoms)
            {
                childOperations.Enqueue(MessageFactory.ToRequestItem(articleBom, requestItem, agent.Context.Self, agent.CurrentTime));

                // create Dispo Agents for to Provide Required Articles
                var agentSetup = AgentSetup.Create(agent, DispoAgent.Behaviour.Factory.Get(SimulationType.None));
                var instruction = Guardian.Instruction.CreateChild.Create(agentSetup, agent.Guardian);
                agent.Send(instruction);
            }
            fArticle = requestItem;
        }

        private void SetHubAgent(Production agent, FHubInformation hub)
        {
            // Enque my Element at Comunication Agent
            agent.DebugMessage("Recived Agent from Directory: " + agent.Sender.Path.Name);

            // add agent to current Scope.
            hubAgents.Add(hub.Ref, hub.RequiredFor);
            // foreach fitting WorkSchedule
            foreach (var workItem in operationList.Where(x => x.Operation.ResourceSkill.Name == hub.RequiredFor))
            {
                agent.Send(Hub.Instruction.EnqueueJob.Create(workItem, hub.Ref));
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

        internal void RequestHubAgentFor(Agent agent, ICollection<M_Operation> workSchedules)
        {
            // Request Comunication Agent for my Workschedules
            var machineGroups = workSchedules.Select(x => x.ResourceSkill.Name).Distinct().ToList();
            foreach (var machineGroupName in machineGroups)
            {
                agent.Send(Directory.Instruction
                            .RequestRessourceAgent
                            .Create(descriminator: machineGroupName
                                    , target: agent.ActorPaths.HubDirectory.Ref));
            }
        }

        internal void CreateWorkItemsFromRequestItem(bool firstItemToBuild, FArticle requestItem)
        {

        }
    }
}
