using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Master40.Agents.Agents.Internal;
using Master40.Agents.Agents.Model;
using Master40.DB.Models;
using Master40.Tools.Simulation;

namespace Master40.Agents.Agents
{
    public class MachineAgent : Agent
    {

        // Agent to register your Services
        private readonly DirectoryAgent _directoryAgent;
        private ComunicationAgent _comunicationAgent;
        private Machine Machine { get; }
        private int ProgressQueueSize { get; }
        public List<WorkItem> Queue { get; }
        //private Queue<WorkItem> SchduledQueue;
        private LimitedQueue<WorkItem> ProcessingQueue { get; }
        /// <summary>
        /// planing forecast, to drop requests over this value
        /// </summary>
        private int QueueLength { get; }
        private bool ItemsInProgess { get; set; }
        private WorkTimeGenerator WorkTimeGenerator { get; }

        public enum InstuctionsMethods
        {
            SetComunicationAgent,
            RequestProposal,
            AcknowledgeProposal,
            StartWorkWith,
            DoWork,
            FinishWork
        }

        public MachineAgent(Agent creator, string name, bool debug, DirectoryAgent directoryAgent, Machine machine, WorkTimeGenerator workTimeGenerator) : base(creator, name, debug)
        {
            _directoryAgent = directoryAgent;
            ProgressQueueSize = 1; // TODO COULD MOVE TO MODEL for CONFIGURATION, May not required anymore
            Queue = new List<WorkItem>();  // ThenBy( x => x.Status)
            ProcessingQueue = new LimitedQueue<WorkItem>(1);
            Machine = machine;
            WorkTimeGenerator = workTimeGenerator;
            ItemsInProgess = false;
            RegisterService();
            QueueLength = 45; // plaing forecast
        }


        /// <summary>
        /// Register the Machine in the System on Startup.
        /// </summary>
        public void RegisterService()
        {
            _directoryAgent.InstructionQueue.Enqueue(new InstructionSet
            {
                MethodName = DirectoryAgent.InstuctionsMethods.GetOrCreateComunicationAgentForType.ToString(),
                ObjectToProcess = Machine.MachineGroup.Name,
                ObjectType = typeof(string),
                SourceAgent = this
            });
        }

        /// <summary>
        /// Callback
        /// </summary>
        /// <param name="objects"></param>
        private void SetComunicationAgent(InstructionSet objects)
        {
            // save Cast to expected object
            _comunicationAgent  = objects.ObjectToProcess as ComunicationAgent;

            // throw if cast failed.
            if (_comunicationAgent == null)
                throw new ArgumentException("Could not Cast Communication Agent from InstructionSet.ObjectToProcess");

            // Debug Message
            DebugMessage("Successfull Registred Service at : " + _comunicationAgent.Name);
        }

        /// <summary>
        /// Is Called from Comunication Agent to get an Proposal when the item with a given priority can be scheduled.
        /// </summary>
        /// <param name="instructionSet"></param>
        private void RequestProposal(InstructionSet instructionSet)
        {
            var workItem = instructionSet.ObjectToProcess as WorkItem;
            if (workItem == null)
                throw new InvalidCastException("Could not Cast Workitem on InstructionSet.ObjectToProcess");
         
            // debug
            DebugMessage("Request for Proposal");
            // Send
            SendProposalTo(instructionSet.SourceAgent, workItem);
        }


        /// <summary>
        /// Send Proposal to Comunication Client
        /// </summary>
        /// <param name="targetAgent"></param>
        /// <param name="workItem"></param>
        private void SendProposalTo(Agent targetAgent, WorkItem workItem)
        {
            var max = 0;
            if(Queue.Any(e => e.Priority(Context.TimePeriod) <= workItem.Priority(Context.TimePeriod)))
            {
                max = Queue.Where(e => e.Priority(Context.TimePeriod) <= workItem.Priority(Context.TimePeriod)).Max(e => e.EstimatedEnd);
            }

            // calculat Proposal.
            var proposal = new Proposal
            {
                AgentId = this.AgentId,
                WorkItemId = workItem.Id,
                Postponed = (max > QueueLength && workItem.Status != Status.Ready), // bool to postpone the item for later, exept it is already Ready
                PostponedFor = QueueLength,
                PossibleSchedule = max
            };
            // callback 
            CreateAndEnqueueInstuction(methodName: ComunicationAgent.InstuctionsMethods.ProposalFromMachine.ToString(),
                                    objectToProcess: proposal,
                                    targetAgent: targetAgent);
        }

        /// <summary>
        /// is Called if The Proposal is accepted by Comunication Agent
        /// </summary>
        /// <param name="instructionSet"></param>
        private void AcknowledgeProposal(InstructionSet instructionSet)
        {
            var workItem = instructionSet.ObjectToProcess as WorkItem;
            if (workItem == null)
                throw new InvalidCastException("Could not Cast Workitem on InstructionSet.ObjectToProcess");

            if (Queue.Any(e => e.Priority(Context.TimePeriod) <= workItem.Priority(Context.TimePeriod)))
            {
                // Get item Latest End.
                var maxItem = Queue.Where(e => e.Priority(Context.TimePeriod) <= workItem.Priority(Context.TimePeriod)).Max(e => e.EstimatedEnd);

                // check if Queuable
                if (maxItem > workItem.EstimatedStart)
                {
                    // reset Agent Status
                    workItem.Status = Status.Created;
                    workItem.EstimatedStart = 0;
                    SendProposalTo(instructionSet.SourceAgent, workItem);
                    return;
                }
            }

            DebugMessage("AcknowledgeProposal and Enqueued Item: " + workItem.WorkSchedule.Name);
            Queue.Add(workItem);
            
            // Enqued before another item?
            var position = Queue.OrderBy(x => x.Priority(Context.TimePeriod)).ToList().IndexOf(workItem);
            DebugMessage("Position: " + position + " Priority:"+ workItem.Priority(Context.TimePeriod) + " Queue length " + Queue.Count());

            // reorganize Queue if an Element has ben Queued which is More Important.
            if (position + 1 < Queue.Count)
            {
                var toRequeue = Queue.OrderBy(x => x.Priority(Context.TimePeriod)).ToList().GetRange(position + 1, Queue.Count() - position - 1);

                CallToReQueue(toRequeue);

                DebugMessage("New Queue length = " + Queue.Count);
            }


            if (workItem.Status == Status.Ready)
            {
                // update Processing queue
                UpdateProcessingQueue(workItem);

                // there is at least Something Ready so Start Work
                DoWork(new InstructionSet());
            }
        }

        private void CallToReQueue(IEnumerable<WorkItem> toRequeue)
        {
            foreach (var reqItem in toRequeue)
            {
                DebugMessage("-> ToRequeue " + reqItem.Priority(Context.TimePeriod) + " Current Possition: " + Queue.OrderBy(x => x.Priority(Context.TimePeriod)).ToList().IndexOf(reqItem) + " Id " + reqItem.Id);

                // remove item from current Queue
                Queue.Remove(reqItem);
                // reset Agent Status
                reqItem.Status = Status.Created;

                // Call Comunication Agent to Requeue
                CreateAndEnqueueInstuction(methodName: ComunicationAgent.InstuctionsMethods.EnqueueWorkItem.ToString(),
                    objectToProcess: reqItem,
                    targetAgent: reqItem.ComunicationAgent);
            }
        }

        private void StartWorkWith(InstructionSet instructionSet)
        {
            var workItemStatus = instructionSet.ObjectToProcess as WorkItemStatus;
            if (workItemStatus == null)
            {
                throw new InvalidCastException("Could not Cast >WorkItemStatus< on InstructionSet.ObjectToProcess");
            }


            // update Status
            var workItem = Queue.FirstOrDefault(x => x.Id == workItemStatus.WorkItemId);
         
            if (workItem != null && workItem.Status == Status.Ready)
            {
                DebugMessage("Set Item: " + workItem.WorkSchedule.Name + " | Status to: " + workItem.Status);
                // update Processing queue
                UpdateProcessingQueue(workItem);
                
                // there is at least Something Ready so Start Work
                DoWork(new InstructionSet());
            }
        }

        private void UpdateProcessingQueue(WorkItem workItem)
        {
            if (ProcessingQueue.CapacitiesLeft && workItem != null)
            {
                CreateAndEnqueueInstuction(methodName: ProductionAgent.InstuctionsMethods.ProductionStarted.ToString(),
                                      objectToProcess: workItem,
                                          targetAgent: workItem.ProductionAgent);
                ProcessingQueue.Enqueue(workItem);
                Queue.Remove(workItem);
            }
        }

        private void DoWork(InstructionSet instructionSet)
        {
            if (ItemsInProgess) { 
                DebugMessage("Im still working....");
                return; // still working
            }

            // Dequeue
            var item = ProcessingQueue.Dequeue();


            // Wait if nothing More todo.
            if (item == null)
            {
                // No more work 
                DebugMessage("Nothing more Ready in Queue!");
                return;
            }
            
            DebugMessage("Start With " +  item.WorkSchedule.Name);
            ItemsInProgess = true;
            item.Status = Status.Processed;


            // TODO: Roll delay here
            var duration = WorkTimeGenerator.GetRandomWorkTime(item.WorkSchedule.Duration);

            //Debug.WriteLine("Duration: " + duration + " for " + item.WorkSchedule.Name);
            Statistics.UpdateSimulationWorkSchedule(item.Id.ToString(), (int)Context.TimePeriod, duration - 1, this.Machine);
            
            // get item = ready and lowest priority
            CreateAndEnqueueInstuction(methodName: MachineAgent.InstuctionsMethods.FinishWork.ToString(),
                                  objectToProcess: item,
                                      targetAgent: this,
                                          waitFor: duration);
        }

        private void FinishWork(InstructionSet instructionSet)
        {
            var item = instructionSet.ObjectToProcess as WorkItem;
            if (item == null)
            {
                throw new InvalidCastException("Could not Cast >WorkItemStatus< on InstructionSet.ObjectToProcess");
            }

            // Set next Ready Element from Queue
            var itemFromQueue = Queue.Where(x => x.Status == Status.Ready).OrderBy(x => x.Priority(Context.TimePeriod)).FirstOrDefault();
            UpdateProcessingQueue(itemFromQueue);

            // Set Machine State to Ready for next
            ItemsInProgess = false;
            DebugMessage("Finished Work with " + item.WorkSchedule.Name + " take next...");

            // Call Comunication Agent that item has ben processed.
            CreateAndEnqueueInstuction(methodName: ComunicationAgent.InstuctionsMethods.FinishWorkItem.ToString(),
                                  objectToProcess: new WorkItemStatus { CurrentPriority = 0,
                                                                        Status = Status.Finished,
                                                                        WorkItemId = item.Id },
                                      targetAgent: item.ComunicationAgent);

            // Reorganize List
            CallToReQueue(Queue.Where(x => x.Status == Status.Created || x.Status == Status.InQueue).ToList());
            // do Do Work in next Timestep.
            CreateAndEnqueueInstuction(methodName: InstuctionsMethods.DoWork.ToString(),
                                  objectToProcess: new InstructionSet(),
                                      targetAgent: this );//,
                                         // waitFor: 1);
            //DoWork(new InstructionSet());
        }

    }
}