using System;
using System.Collections.Generic;
using System.Linq;
using Master40.Agents.Agents.Internal;
using Master40.Agents.Agents.Model;
using Microsoft.EntityFrameworkCore.Design;

namespace Master40.Agents.Agents
{
    public class ComunicationAgent : Agent
    {

        private List<WorkItem> WorkItemQueue;
        private List<Agent> MachineAgents;
        
        public ComunicationAgent(Agent creator, string name, bool debug, string contractType) 
            : base(creator, name, debug)
        {
            ContractType = contractType;
            WorkItemQueue = new List<WorkItem>();
            MachineAgents = new List<Agent>();
        }
        public string ContractType { get; set; }

        public enum InstuctionsMethods
        {
            AddMachineToComunicationAgent,
            EnqueueWorkItem,
            ProposalFromMachine,
            SetWorkItemStatus
        }

        private void EnqueueWorkItem(InstructionSet instructionSet)
        {
            var workItem = instructionSet.ObjectToProcess as WorkItem;
            if (workItem == null)
            {
                throw new InvalidCastException("Could not Cast Workitem on InstructionSet.ObjectToProcess");
            }

            // If item is not Already in Queue Add item to Queue
            // // happens i.e. Machine calls to Requeue item.
            if (!WorkItemQueue.Contains(workItem))
            {
                // Set Comunication agent.
                workItem.ComunicationAgent = this;
                // add TO queue
                WorkItemQueue.Add(workItem);
                DebugMessage("Got Item to Enqueue: " + workItem.WorkSchedule.Name + "| with status:" + workItem.Status);
            }
            else
            {
                // reset Item.
                DebugMessage("Got Item to Requeue: " + workItem.WorkSchedule.Name + "| with status:" + workItem.Status);
                workItem.EsitamtedEnd = 0;
                workItem.Proposals.Clear();
            }
            
            foreach (var agent in MachineAgents)
            {
                CreateAndEnqueueInstuction(methodName: MachineAgent.InstuctionsMethods.RequestProposal.ToString(),
                                      objectToProcess: workItem,
                                          targetAgent: agent);
            }



        }

        private void SetWorkItemStatus(InstructionSet instructionSet)
        {
            var workItem = instructionSet.ObjectToProcess as WorkItem;
            if (workItem == null)
            {
                throw new InvalidCastException("Could not Cast Workitem on InstructionSet.ObjectToProcess");
            }
            // update Status
            WorkItemQueue.FirstOrDefault(x => x.Id == workItem.Id).Status = workItem.Status;
            
            DebugMessage("Set Item: " + workItem.WorkSchedule.Name + " | Status to: " + workItem.Status);
            // if 
            if (workItem.Status == Status.Ready)
            {
                // Call for Work 
                /*
                CreateAndEnqueueInstuction(methodName: StorageAgent.InstuctionsMethods.RequestArticle.ToString(),
                                      objectToProcess: RequestItem,
                                          targetAgent: storeAgent);
                */
            }
        }

        private void AddMachineToComunicationAgent(InstructionSet instructionSet)
        {
            var machine = instructionSet.ObjectToProcess as MachineAgent;
            if (machine == null)
            {
                throw new InvalidCastException("Could not Cast MachineAgent on InstructionSet.ObjectToProcess");
            }
            // Add Machine to Pool
            this.MachineAgents.Add(machine);
            // Added Machine Agent To Machine Pool
            DebugMessage("Added Machine Agent "+ machine.Name +" to Machine Pool: " + ContractType);

        }

        private void ProposalFromMachine(InstructionSet instructionSet)
        {
            var proposal = instructionSet.ObjectToProcess as Proposal;
            if (proposal == null)
            {
                throw new InvalidCastException("Could not Cast Proposal on InstructionSet.ObjectToProcess");
            }
            // get releated workitem and add Proposal.
            var workItem = WorkItemQueue.First(x => x.Id == proposal.WorkItemId);
            workItem.Proposals.Add(proposal);

            DebugMessage("Proposal for Schedule: " + proposal.PossibleSchedule + " from: " + proposal.AgentId + "!");


            // if all Machines Answered
            if (workItem.Proposals.Count == MachineAgents.Count)
            {
                // aknowledge Machine -> therefore get Machine -> send aknowledgement
                var acknowledgement = workItem.Proposals.First(x => x.PossibleSchedule == workItem.Proposals.Min(p => p.PossibleSchedule));
                var machineAgent = MachineAgents.First(x => x.AgentId == acknowledgement.AgentId);

                workItem.Status = Status.InQueue;
                workItem.EsitamtedEnd = proposal.PossibleSchedule + workItem.WorkSchedule.Duration;

                CreateAndEnqueueInstuction(methodName: MachineAgent.InstuctionsMethods.AcknowledgeProposal.ToString(),
                                      objectToProcess: workItem,
                                          targetAgent: machineAgent);
            }


        }


    }
}