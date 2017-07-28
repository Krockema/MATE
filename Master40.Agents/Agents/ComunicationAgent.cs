using System;
using System.Collections.Generic;
using System.Linq;
using Master40.Agents.Agents.Internal;
using Master40.Agents.Agents.Model;

namespace Master40.Agents.Agents
{
    public class ComunicationAgent : Agent
    {

        private List<WorkItem> WorkItemQueue;

        public ComunicationAgent(Agent creator, string name, bool debug, string contractType) 
            : base(creator, name, debug)
        {
            ContractType = contractType;
            WorkItemQueue = new List<WorkItem>();
        }
        public string ContractType { get; set; }

        public enum InstuctionsMethods
        {
            EnqueueWorkItem,
            SetWorkItemStatus
        }

        private void EnqueueWorkItem(InstructionSet instructionSet)
        {
            var workItem = instructionSet.ObjectToProcess as WorkItem;
            if (workItem == null)
            {
                throw new InvalidCastException("Could not Cast Workitem on InstructionSet.ObjectToProcess");
            }
            WorkItemQueue.Add(workItem);

            DebugMessage("Got Item to Enqueue: " + workItem.WorkSchedule.Name + "| with status:" + workItem.Status);

            //TODO DO Somethign ? 

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
        }

    }
}