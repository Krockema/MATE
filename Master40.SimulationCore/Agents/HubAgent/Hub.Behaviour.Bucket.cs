using System;
using System.Collections.Generic;
using System.Linq;
using Akka.Actor;
using Master40.SimulationCore.Agents.HubAgent.Buckets.Instructions;
using Master40.SimulationCore.Agents.ProductionAgent;
using Master40.SimulationCore.Agents.Ressource;
using Master40.SimulationCore.MessageTypes;
using Master40.SimulationImmutables;

namespace Master40.SimulationCore.Agents.HubAgent
{
    public class HubBehaviourBucket : Behaviour
    {
        private HubBehaviourBucket(Dictionary<string, object> properties) : base(null, properties) { }

        public static IBehaviour Get(string skillGroup)
        {
            var properties = new Dictionary<string, object>();

            properties.Add(Hub.Properties.WORK_ITEM_QUEUE, new List<FWorkItem>());
            properties.Add(Hub.Properties.BUCKETS, new List<FBucket>());
            properties.Add(Hub.Properties.RESOURCE_AGENTS, new Dictionary<IActorRef, string>());
            properties.Add(Hub.Properties.SKILL_GROUP, skillGroup);

            return new HubBehaviourBucket(properties);
        }

        public override bool Action(Agent agent, object message)
        {
            switch (message)
            {
                case AddWorkItemToBucket msg: Buckets.Behaviour.AddWorkItemToBucket.Action((Hub)agent, msg.GetObjectFromMessage); break;
                case Hub.Instruction.EnqueueBucket msg: EnqueueBucket((Hub)agent, msg.GetObjectFromMessage); break;
                case Hub.Instruction.ProductionStarted msg: ProductionStarted((Hub)agent, msg.GetObjectfromMessage); break;
                case Hub.Instruction.FinishWorkItem msg: FinishWorkItem((Hub)agent, msg.GetObjectFromMessage); break;
                case Hub.Instruction.ProposalFromMachine msg: ProposalFromMachine((Hub)agent, msg.GetObjectFromMessage); break;
                case Hub.Instruction.SetWorkItemStatus msg: SetWorkItemStatus((Hub)agent, msg.GetObjectFromMessage); break;
                case Hub.Instruction.AddMachineToHub msg: AddMachineToHub((Hub)agent, msg.GetObjectFromMessage); break;
                case BasicInstruction.ResourceBrakeDown msg: ResourceBreakDown((Hub)agent, msg.GetObjectFromMessage); break;
                default: return false;
            }
            return true;
        }

        private void ResourceBreakDown(Hub agent, FBreakDown breakDown)
        {
            var machineAgents = agent.Get<Dictionary<IActorRef, string>>(Hub.Properties.RESOURCE_AGENTS);
            var brockenMachine = machineAgents.Single(x => breakDown.Machine == x.Value).Key;
            machineAgents.Remove(brockenMachine);
            agent.Send(BasicInstruction.ResourceBrakeDown.Create(breakDown, brockenMachine, true), 45);

            System.Diagnostics.Debug.WriteLine("Break for " + breakDown.Machine, "Hub");
        }

    

        public void EnqueueBucket(Hub agent, FBucket bucket)
        {
            if (bucket == null)
            {
                throw new InvalidCastException("Could not Cast Bucket on InstructionSet.ObjectToProcess");
            }

            agent.enqueueBucket(bucket);

        }

        public void ProductionStarted(Hub agent, FBucket bucketItem)
        {
            var bucketQueue = agent.Get<List<FBucket>>(Hub.Properties.BUCKETS);
            var temp = bucketQueue.Single(x => x.Key == bucketItem.Key);

            foreach (var item in temp.Operations)
            {
                agent.Send(Production.Instruction
                                          .ProductionStarted
                                          .Create(message: item
                                                 , target: item.ProductionAgent));
            }

            bucketQueue.Replace(temp);
        }

        public void FinishWorkItem(Hub agent, FWorkItem workItem)
        {
            if (workItem == null)
            {
                throw new InvalidCastException("Could not Cast >WorkItemStatus< on InstructionSet.ObjectToProcess");
            }

            var workItemQueue = agent.Get<List<FWorkItem>>(Hub.Properties.WORK_ITEM_QUEUE);

            agent.DebugMessage("Resource called " + workItem.Operation.Name + " finished.");
            agent.Send(Production.Instruction.FinishWorkItem.Create(workItem, workItem.ProductionAgent));
            workItemQueue.Remove(workItemQueue.Find(x => x.Key == workItem.Key));
        }

        public void SetWorkItemStatus(Hub agent, FItemStatus workItemStatus)
        {
            var bucketQueue = agent.Get<List<FBucket>>(Hub.Properties.BUCKETS);

            if (workItemStatus == null)
            {
                throw new InvalidCastException("Could not Cast >WorkItemStatus< on InstructionSet.ObjectToProcess");
            }

            //bucketQueue.ForEach(x => x.Operations.ToList().Find(x => x.Key == workItemStatus.ItemId).UpdateStatus(workItemStatus.Status));

            agent.DebugMessage("Set Item: " + workItem.Operation.Name + " | Status to: " + workItem.Status);
            // if 
            if (workItem.Status == ElementStatus.Ready)
            {
                // Call for Work 
                workItem = workItem.UpdateMaterialsProvided(true).SetReady;

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

        private void ProposalFromMachine(Hub agent, FProposal proposal)
        {
            var bucketQueue = agent.Get<List<FBucket>>(Hub.Properties.BUCKETS);
            var resourceAgents = agent.Get<Dictionary<IActorRef, string>>(Hub.Properties.RESOURCE_AGENTS);

            if (proposal == null)
            {
                throw new InvalidCastException("Could not Cast Proposal on InstructionSet.ObjectToProcess");
            }

            // get releated workitem and add Proposal.
            var bucketItem = bucketQueue.Find(x => x.Key == proposal.BucketId);
            var proposalToRemove = bucketItem.Proposals.Find(x => x.ResourceAgent == proposal.ResourceAgent);
            if (proposalToRemove != null)
            {
                bucketItem.Proposals.Remove(proposalToRemove);
            }

            bucketItem.Proposals.Add(proposal);

            agent.DebugMessage("Proposal for Schedule: " + proposal.PossibleSchedule + " from: " + proposal.ResourceAgent + "!");


            // if all Machines Answered
            if (bucketItem.Proposals.Count == resourceAgents.Count)
            {

                // item Postponed by All Machines ? -> reque after given amount of time.
                if (bucketItem.Proposals.TrueForAll(x => x.Postponed))
                {
                    // Call Hub Agent to Requeue
                    bucketItem = bucketItem.UpdateResourceAgent(ActorRefs.NoSender)
                                       .UpdateStatus(ElementStatus.Created);
                    bucketQueue.Replace(bucketItem);

                    agent.Send(Hub.Instruction.EnqueueBucket.Create(bucketItem, agent.Context.Self), proposal.PostponedFor);
                    return;
                }
                else // updateStatus
                {

                    bucketItem = bucketItem.UpdateStatus(bucketItem.WasSetReady ? ElementStatus.Ready : ElementStatus.InQueue);
                }

                // aknowledge Machine -> therefore get Machine -> send aknowledgement
                var acknowledgement = bucketItem.Proposals.First(x => x.PossibleSchedule == bucketItem.Proposals.Where(y => y.Postponed == false)
                                                                                                            .Min(p => p.PossibleSchedule)
                                                                 && x.Postponed == false);

                // set Proposal Start for Machine to Reque if time slot is closed.
                bucketItem = bucketItem.UpdateEstimations(acknowledgement.PossibleSchedule, acknowledgement.ResourceAgent);
                bucketQueue.Replace(bucketItem);
                agent.Send(Resource.Instruction.AcknowledgeProposal.Create(bucketItem, acknowledgement.ResourceAgent));
            }
        }


        private void AddMachineToHub(Hub agent, FHubInformation hubInformation)
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
