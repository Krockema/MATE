using System;
using System.Collections.Generic;
using System.Linq;
using Akka.Actor;
using Master40.SimulationCore.Agents.ProductionAgent;
using Master40.SimulationCore.Agents.Ressource;
using Master40.SimulationCore.MessageTypes;
using Master40.SimulationImmutables;

namespace Master40.SimulationCore.Agents.HubAgent
{
    public class HubBehaviour : Behaviour
    {
        private HubBehaviour(Dictionary<string, object> properties) : base(null, properties) { }

        public static HubBehaviour Get(string skillGroup)
        {
            var properties = new Dictionary<string, object>();

            properties.Add(Hub.Properties.WORK_ITEM_QUEUE, new List<FWorkItem>());
            properties.Add(Hub.Properties.RESOURCE_AGENTS, new Dictionary<IActorRef, string>());
            properties.Add(Hub.Properties.SKILL_GROUP, skillGroup);

            return new HubBehaviour(properties);
        }

        public override bool Action(Agent agent, object message)
        {
            switch (message)
            {
                case Hub.Instruction.EnqueueWorkItem msg: EnqueueWorkItem(agent, msg.GetObjectFromMessage); break;
                case Hub.Instruction.ProductionStarted msg: ProductionStarted(agent, msg.GetObjectfromMessage); break;
                case Hub.Instruction.FinishWorkItem msg: FinishWorkItem(agent, msg.GetObjectFromMessage); break;
                case Hub.Instruction.ProposalFromMachine msg: ProposalFromMachine(agent, msg.GetObjectFromMessage); break;
                case Hub.Instruction.SetWorkItemStatus msg: SetWorkItemStatus(agent, msg.GetObjectFromMessage); break;
                case Hub.Instruction.AddMachineToHub msg: AddMachineToHub(agent, msg.GetObjectFromMessage); break;
                case BasicInstruction.ResourceBrakeDown msg: ResourceBreakDown(agent, msg.GetObjectFromMessage); break;
                default: return false;
            }
            return true;
        }

        private void ResourceBreakDown(Agent agent, FBreakDown breakDown)
        {
            var machineAgents = agent.Get<Dictionary<IActorRef, string>>(Hub.Properties.RESOURCE_AGENTS);
            var brockenMachine = machineAgents.Single(x => breakDown.Machine == x.Value).Key;
            machineAgents.Remove(brockenMachine);
            agent.Send(BasicInstruction.ResourceBrakeDown.Create(breakDown, brockenMachine, true), 45);

            System.Diagnostics.Debug.WriteLine("Break for " + breakDown.Machine, "Hub");
        }

        private void EnqueueWorkItem(Agent agent, FWorkItem workItem)
        {

            var workItemQueue = agent.Get<List<FWorkItem>>(Hub.Properties.WORK_ITEM_QUEUE);
            var machineAgents = agent.Get<Dictionary<IActorRef, string>>(Hub.Properties.RESOURCE_AGENTS);
            var localItem = workItemQueue.Find(x => x.Key == workItem.Key);
            // If item is not Already in Queue Add item to Queue
            // // happens i.e. Machine calls to Requeue item.
            if (localItem == null)
            {
                // Set Comunication agent.
                localItem = workItem.UpdateHubAgent(agent.Context.Self);
                // add TO queue
                workItemQueue.Add(localItem);
                agent.DebugMessage("Got New Item to Enqueue: " + workItem.Operation.Name + " | with status:" + workItem.Status + " with Id: " + workItem.Key);
            }
            else
            {
                // reset Item.
                agent.DebugMessage("Got Item to Requeue: " + workItem.Operation.Name + " | with status:" + workItem.Status + " with Id: " + workItem.Key);
                workItem.Proposals.Clear();
                localItem = workItem.UpdateHubAgent(agent.Context.Self);
                workItemQueue.Replace(localItem);
            }

            foreach (var actorRef in machineAgents)
            {
                agent.Send(instruction: Resource.Instruction.RequestProposal.Create(localItem, actorRef.Key));
            }
        }

        public void ProductionStarted(Agent agent, FWorkItem workItem)
        {
            var workItemQueue = agent.Get<List<FWorkItem>>(Hub.Properties.WORK_ITEM_QUEUE);
            var temp = workItemQueue.Single(x => x.Key == workItem.Key);
            var tempItem = workItem.UpdatePoductionAgent(temp.ProductionAgent);
            agent.Send(Production.Instruction
                                 .ProductionStarted
                                 .Create(message: tempItem
                                        , target: temp.ProductionAgent));
            workItemQueue.Replace(tempItem);
        }

        public void FinishWorkItem(Agent agent, FWorkItem workItem)
        {
            if (workItem == null)
            {
                throw new InvalidCastException("Could not Cast >WorkItemStatus< on InstructionSet.ObjectToProcess");
            }

            var workItemQueue = agent.Get<List<FWorkItem>>(Hub.Properties.WORK_ITEM_QUEUE);

            agent.DebugMessage("Machine called Item " + workItem.Operation.Name + " with Id: " + workItem.Key + " finished.");
            agent.Send(Production.Instruction.FinishWorkItem.Create(workItem, workItem.ProductionAgent));
            workItemQueue.Remove(workItemQueue.Find(x => x.Key == workItem.Key));
        }

        public void SetWorkItemStatus(Agent agent, FItemStatus workItemStatus)
        {
            var workItemQueue = agent.Get<List<FWorkItem>>(Hub.Properties.WORK_ITEM_QUEUE);

            if (workItemStatus == null)
            {
                throw new InvalidCastException("Could not Cast >WorkItemStatus< on InstructionSet.ObjectToProcess");
            }
            // update Status
            var workItem = workItemQueue.Find(x => x.Key == workItemStatus.ItemId)
                                         .UpdateStatus(workItemStatus.Status);

            agent.DebugMessage("Set Item: " + workItem.Operation.Name + " | Status to: " + workItem.Status + " with Id: " + workItem.Key);
            // if 
            if (workItem.Status == ElementStatus.Ready)
            {
                // Call for Work 
                workItem = workItem.UpdateMaterialsProvided(true);

                // Check if this item has a corrosponding mashine slot
                if (workItem.ResourceAgent == ActorRefs.NoSender)
                {
                    // If not Call Comunication Agent to Requeue
                    // do Nothing 
                    // CreateAndEnqueueInstuction(methodName: ComunicationAgent.InstuctionsMethods.EnqueueWorkItem.ToString(),
                    //     objectToProcess: workItem,
                    //     targetAgent: workItem.ComunicationAgent);
                }
                else
                {
                    agent.Send(Resource.Instruction.StartWorkWith.Create(workItemStatus, workItem.ResourceAgent));
                    agent.DebugMessage("Call for Work");
                }
            }
            workItemQueue.Replace(workItem);
        }

        private void ProposalFromMachine(Agent agent, FProposal proposal)
        {
            var workItemQueue = agent.Get<List<FWorkItem>>(Hub.Properties.WORK_ITEM_QUEUE);
            var machineAgents = agent.Get<Dictionary<IActorRef, string>>(Hub.Properties.RESOURCE_AGENTS);

            if (proposal == null)
            {
                throw new InvalidCastException("Could not Cast Proposal on InstructionSet.ObjectToProcess");
            }
            // get releated workitem and add Proposal.
            var workItem = workItemQueue.Find(x => x.Key == proposal.WorkItemId);
                workItem.Proposals.RemoveAll(x => x.ResourceAgent == proposal.ResourceAgent);
            // add New Proposal
            workItem.Proposals.Add(proposal);

            agent.DebugMessage("Proposal for Schedule: " + proposal.PossibleSchedule + " Id: " + proposal.WorkItemId + " from:" + proposal.ResourceAgent + "!");


            // if all Machines Answered
            if (workItem.Proposals.Count == machineAgents.Count)
            {

                // item Postponed by All Machines ? -> reque after given amount of time.
                if (workItem.Proposals.TrueForAll(x => x.Postponed))
                {
                    // Call Hub Agent to Requeue
                    workItem = workItem.UpdateResourceAgent(ActorRefs.NoSender)
                                       .UpdateStatus(ElementStatus.Created);
                    workItemQueue.Replace(workItem);
                    agent.Send(Hub.Instruction.EnqueueWorkItem.Create(workItem, agent.Context.Self), proposal.PostponedFor);
                    return;
                }


                // aknowledge Machine -> therefore get Machine -> send aknowledgement
                workItem = workItem.UpdateStatus(workItem.MaterialsProvided ? ElementStatus.Ready : ElementStatus.InQueue);
                var acknowledgement = workItem.Proposals.First(x => x.PossibleSchedule == workItem.Proposals.Where(y => y.Postponed == false)
                                                                                                            .Min(p => p.PossibleSchedule)
                                                                 && x.Postponed == false);

                agent.DebugMessage("Start AcknowledgeProposal for " + proposal.WorkItemId + " on resource " + acknowledgement.ResourceAgent);

                // set Proposal Start for Machine to Reque if time slot is closed.
                workItem = workItem.UpdateEstimations(acknowledgement.PossibleSchedule, acknowledgement.ResourceAgent);
                workItem.Proposals.Clear();
                workItemQueue.Replace(workItem);
                agent.Send(Resource.Instruction.AcknowledgeProposal.Create(workItem, acknowledgement.ResourceAgent));
            }
        }


        private void AddMachineToHub(Agent agent, FHubInformation hubInformation)
        {
            var machineAgents = agent.Get<Dictionary<IActorRef, string>>(Hub.Properties.RESOURCE_AGENTS);
            if (hubInformation == null)
            {
                throw new InvalidCastException("Could not Cast MachineAgent on InstructionSet.ObjectToProcess - From:" + agent.Sender.Path.Name);
            }
            // Add Machine to Pool
            machineAgents.Add(hubInformation.Ref, hubInformation.RequiredFor);
            // Added Machine Agent To Machine Pool
            agent.DebugMessage("Added Machine Agent " + hubInformation.Ref.Path.Name + " to Machine Pool: " + hubInformation.RequiredFor);
        }
    }
}