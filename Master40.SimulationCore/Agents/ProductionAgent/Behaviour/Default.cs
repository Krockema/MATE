using System;
using System.Collections.Generic;
using System.Linq;
using Akka.Actor;
using Master40.DB.DataModel;
using Master40.SimulationCore.Agents.DirectoryAgent;
using Master40.SimulationCore.Agents.DispoAgent;
using Master40.SimulationCore.Agents.HubAgent;
using Master40.SimulationCore.Agents.StorageAgent;
using Master40.SimulationCore.Helper;
using Master40.SimulationCore.MessageTypes;
using Master40.SimulationImmutables;
using static Master40.SimulationCore.Agents.ProductionAgent.Production.Properties;

namespace Master40.SimulationCore.Agents.ProductionAgent.Behaviour
{
    public class Default : MessageTypes.Behaviour
    {
        private List<FRequestItem> _requestItems;
        internal Default(Dictionary<string, object> properties) : base(null, properties) { }
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
        private void StartProductionAgent(Production agent, FRequestItem requestItem)
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
                RequestHubAgentFor(agent, workSchedules: requestItem.Article.WorkSchedules);
                // And Create workItems
                agent.CreateWorkItemsFromRequestItem(firstItemToBuild: firstToEnqueue, requestItem);
            }

            var childItems = agent.Get<Queue<FRequestItem>>(CHILD_WORKITEMS);
            // Create Dispo Agents for Childs.
            foreach (var articleBom in requestItem.Article.ArticleBoms)
            {
                childItems.Enqueue(MessageFactory.ToRequestItem(articleBom, requestItem, agent.Context.Self));

                // create Dispo Agents for to Provide Required Articles
                var agentSetup = AgentSetup.Create(agent, DispoAgent.Behaviour.BehaviourFactory.Get(DB.Enums.SimulationType.None));
                var instruction = Guardian.Guardian.Instruction.CreateChild.Create(agentSetup, agent.Guardian);
                agent.Send(instruction);
            }
            agent.Set(REQUEST_ITEM, requestItem);
        }

        private void SetHubAgent(Production agent, FHubInformation hub)
        {
            var hubAgents = agent.Get<Dictionary<IActorRef, string>>(HUB_AGENTS);
            var workItems = agent.Get<List<FWorkItem>>(WORK_ITEMS);
            // Enque my Element at Comunication Agent
            if (hub == null)
            {
                throw new InvalidCastException(" Could not Cast Comunication agent on InstructionSet.");
            }
            agent.DebugMessage("Recived Agent from Directory: " + agent.Sender.Path.Name);

            // add agent to current Scope.
            hubAgents.Add(hub.Ref, hub.RequiredFor);
            // foreach fitting WorkSchedule
            foreach (var workItem in workItems.Where(x => x.Operation.ResourceSkill.Name == hub.RequiredFor))
            {
                agent.Send(Hub.Instruction.EnqueueWorkItem.Create(workItem, hub.Ref));
            }
        }
        private void ProductionStarted(Production agent, FWorkItem workItem)
        {
            var status = agent.Get<ElementStatus>(ELEMENT_STATUS);
            if (status != workItem.Status)
            {
                status = ElementStatus.Processed;

                foreach (var child in agent.VirtualChilds)
                {
                    agent.Send(Dispo.Instruction
                                    .WithdrawMaterialsFromStock
                                    .Create(message: "Production Start"
                                            , target: child.Key));
                }
            }

        }

        internal void Finished(Agent agent, FItemStatus itemStatus)
        {
            // // any Not Finished do noting
            // if (ChildAgents.Any(x => x.Status != Status.Finished))
            //     return;
            var workItems = agent.Get<List<FWorkItem>>(WORK_ITEMS);
            var requestItem = agent.Get<FRequestItem>(REQUEST_ITEM);
            var hubAgents = agent.Get<Dictionary<IActorRef, string>>(HUB_AGENTS);
            // Return from Production as WorkItemStatus
            if (itemStatus != null)
            {
                var workItem = workItems.First(x => x.Key == itemStatus.ItemId);
                workItems.Replace(workItem.UpdateStatus(itemStatus.Status));
            }

            // TODO Anything ?
            if (requestItem.Article.WorkSchedules != null && workItems.All(x => x.Status == ElementStatus.Finished))
            {
                agent.Status = ElementStatus.Finished;
                agent.Send(instruction: Storage.Instruction.ResponseFromProduction.Create(requestItem, requestItem.StorageAgent));

                agent.DebugMessage("All Workschedules have been Finished: RequestItem: " + requestItem.Key);
                agent.TryToFinish();
                return;
            }
            // else
            agent.DebugMessage("Im Ready To get Enqued");
            agent.Status = (agent.Status == ElementStatus.Processed) ? ElementStatus.Processed : ElementStatus.Ready;
            SetMaterialsProvided(workItems);
            if (hubAgents.Count > 0)
            {
                SetWorkItemReady(agent);
            }
        }

        private void ProvideRequest(Production agent, FItemStatus itemStatus)
        {
            var requestedItems = agent.Get<List<FRequestItem>>(REQUESTED_ITEMS);
            var workItems = agent.Get<List<FWorkItem>>(WORK_ITEMS);
            var requestItem = requestedItems.Single(x => x.Key == itemStatus.ItemId);
            var status = agent.Get<ElementStatus>(ELEMENT_STATUS);

            //agent.DebugMessage("Item to Remove from requestItems: " + requestItem.Article.Name + " --> left " + (requestedItems.Count()));
            requestedItems.Remove(requestItem);
            System.Diagnostics.Debug.WriteLine(agent.Name + ": Item to Remove from requestItems: " + requestItem.Article.Name + " --> left " + (requestedItems.Count()));

            if (workItems.Any() && requestedItems.Count() == 0)
            {
                SetMaterialsProvided(workItems);
                SetWorkItemReady(agent);
            }
            if (status == ElementStatus.Finished)
            {
                agent.TryToFinish();
            }
        }

        private void SetMaterialsProvided(List<FWorkItem> workItems)
        {
            var updatedMats = new List<FWorkItem>();
            foreach (var item in workItems)
            {
                updatedMats.Add(item.UpdateMaterialsProvided(true));
            }
            workItems.Clear();
            workItems.AddRange(updatedMats);
        }

        private void FinishWorkItem(Agent agent, FWorkItem workItem)
        {
            if (workItem == null)
            {
                throw new InvalidCastException("Could not Cast >WorkItemStatus< on InstructionSet.ObjectToProcess");
            }
            agent.DebugMessage("Machine called finished with: " + workItem.Operation.Name + " workItemId: " + workItem.Key + " !");

            // Shortcut:
            //CreateAndEnqueue
            Finished(agent, new FItemStatus(workItem.Key
                                    , workItem.Status
                                    , workItem.ItemPriority));
        }

        internal void SetWorkItemReady(Agent agent)
        {
            var workItems = agent.Get<List<FWorkItem>>(WORK_ITEMS);
            var hubAgents = agent.Get<Dictionary<IActorRef, string>>(HUB_AGENTS);
            // get next ready WorkItem
            // TODO Return Queing Status ? or Move method to Machine
            var nextItem = workItems.Where(x => x.Status != ElementStatus.Finished 
                                             || x.Status != ElementStatus.Processed)
                                    .OrderBy(x => x.Operation.HierarchyNumber)
                                    .FirstOrDefault();
            if (nextItem == null)
            {
                agent.DebugMessage("Requirement Missmatch! No Folowup found.");
                return;
            }

            var hubAgent = hubAgents.SingleOrDefault((KeyValuePair<IActorRef, string> x) => x.Value == nextItem.Operation.ResourceSkill.Name);
            if (hubAgent.Value == null)
            {
                agent.DebugMessage("No Hub Agent yet:" + nextItem.Status + " workItemId: " + nextItem.Key);
                return;
            }
            SendWorkItemStatusMsg(agent, hubAgent, nextItem);
        }

        private void SendWorkItemStatusMsg(Agent agent, KeyValuePair<IActorRef, string> hubAgent, FWorkItem nextItem)
        {
            agent.DebugMessage("Set next WorkItem Ready from Status " + nextItem.Status + " workItemId: " + nextItem.Key);
            // tell Item in Queue to set it ready.
            agent.Send(instruction: Hub.Instruction.EnqueueWorkItem.Create(nextItem, hubAgent.Key));
            // ,waitFor: 1); // Start Production during the next time period
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
    }
}
