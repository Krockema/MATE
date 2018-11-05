using Akka.Actor;
using Akka.Event;
using Master40.DB.Models;
using Master40.SimulationCore.Helper;
using Master40.SimulationCore.Agents;
using Master40.SimulationCore.Reporting;
using Master40.SimulationImmutables;
using Master40.Tools.Simulation;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Master40.SimulationCore.Agents
{
    // Alt: CapacityAgent, ProducerAgent, PotencialFactorAgent, SchedulingAgent, 
    public partial class ResourceAgent : Agent
    {

        // Agent to register your Services
        
        private IActorRef _hubAgent;
        private Machine Machine { get; }
        private int ProgressQueueSize { get; }
        private List<WorkItem> Queue { get; }
        //private Queue<WorkItem> SchduledQueue;
        private LimitedQueue<WorkItem> ProcessingQueue { get; }
        /// <summary>
        /// planing forecast, to drop requests over this value
        /// </summary>
        private int QueueLength { get; }
        private bool _itemsInProgess { get; set; }
        private WorkTimeGenerator _workTimeGenerator { get; }



        // public Constructor
        public static Props Props(ActorPaths actorPaths,Machine machine, WorkTimeGenerator workTimeGenerator, long time, bool debug)
        {
            return Akka.Actor.Props.Create(() => new ResourceAgent(actorPaths, machine, workTimeGenerator, time, debug));
        }

        public ResourceAgent(ActorPaths actorPaths, Machine machine, WorkTimeGenerator workTimeGenerator, long time, bool debug) : base(actorPaths, time, debug)
        {
            ProgressQueueSize = 1; // TODO COULD MOVE TO MODEL for CONFIGURATION, May not required anymore
            Queue = new List<WorkItem>();  // ThenBy( x => x.Status)
            ProcessingQueue = new LimitedQueue<WorkItem>(1);
            Machine = machine;
            _workTimeGenerator = workTimeGenerator;
            _itemsInProgess = false;
            RegisterService();
            QueueLength = 45; // plaing forecast
        }

        private void RegisterService()
        {
            var hubRequest = new HubInformation(ResourceType.Machine, Machine.MachineGroup.Name, ActorRefs.NoSender);
            // not required due to the Directory Agent Creates the Machiens
            //CreateAndEnqueue(DirectoryAgent.Instruction.GetOrCreateHubAgentForType.Create(hubRequest, ActorPaths.HubDirectory.Ref));
        }

       // protected override void Do(object o)
       // {
       //     switch (o)
       //     {
       //         case BasicInstruction.Initialize i: RegisterService(); break;
       //         case Instruction.SetHubAgent instruction: SetHubAgent(instruction.GetObjectFromMessage.Ref); break;
       //         case Instruction.RequestProposal instruction: RequestProposal(instruction.GetObjectFromMessage); break;
       //         case Instruction.AcknowledgeProposal instruction: AcknowledgeProposal(instruction.GetObjectFromMessage); break;
       //         case Instruction.StartWorkWith instruction: StartWorkWith(instruction.GetObjectFromMessage); break;
       //         case Instruction.DoWork instruction: DoWork(); break;
       //         case Instruction.FinishWork instruction: FinishWork(instruction.GetObjectFromMessage); break;
       //         default: throw new Exception("Unhandeld"); 
       //     }
       // }
       //
        private void StartWorkWith(ItemStatus workItemStatus)
        {
            
            if (workItemStatus == null)
            {
                throw new InvalidCastException("Could not Cast >WorkItemStatus< on InstructionSet.ObjectToProcess");
            }


            // update Status
            var workItem = Queue.FirstOrDefault(x => x.Key == workItemStatus.ItemId);
            
                

            if (workItem != null && workItemStatus.Status == ElementStatus.Ready)
            {
                Queue.Remove(workItem);
                workItem = workItem.UpdateStatus(workItemStatus.Status);
                Queue.Add(workItem);

                DebugMessage("Set Item: " + workItem.WorkSchedule.Name + " | Status to: " + workItem.Status);
                // update Processing queue
                UpdateProcessingQueue(workItem);

                // there is at least Something Ready so Start Work
                DoWork();
            }
        }

        private void FinishWork(WorkItem item)
        {
            
            if (item == null)
            {
                throw new InvalidCastException("Could not Cast >WorkItemStatus< on InstructionSet.ObjectToProcess");
            }

            // Set next Ready Element from Queue
            var itemFromQueue = Queue.Where(x => x.Status == ElementStatus.Ready).OrderBy(x => x.Priority(TimePeriod)).ThenBy(x => x.WorkSchedule.Duration).FirstOrDefault();
            UpdateProcessingQueue(itemFromQueue);

            // Set Machine State to Ready for next
            _itemsInProgess = false;
            DebugMessage("Finished Work with " + item.WorkSchedule.Name + " take next...");

            item = item.UpdateStatus(ElementStatus.Finished);

            // Call Hub Agent that item has ben processed.
            CreateAndEnqueue(HubAgent.Instruction.FinishWorkItem.Create(item, item.HubAgent));
                
                
            // Reorganize List
            CallToReQueue(Queue.Where(x => x.Status == ElementStatus.Created || x.Status == ElementStatus.InQueue).ToList());
            // do Do Work in next Timestep.
            CreateAndEnqueue(Instruction.DoWork.Create(null, Self));
        }

        private void UpdateProcessingQueue(WorkItem workItem)
        {
            if (ProcessingQueue.CapacitiesLeft && workItem != null)
            {
                if(workItem.WorkSchedule.HierarchyNumber == 10)
                    CreateAndEnqueue(HubAgent.Instruction.ProductionStarted.Create(workItem, _hubAgent));
                ProcessingQueue.Enqueue(workItem);
                Queue.Remove(workItem);
            }
        }

        private void CallToReQueue(List<WorkItem> toRequeue)
        {
            foreach (var reqItem in toRequeue)
            {
                DebugMessage("-> ToRequeue " + reqItem.Priority(TimePeriod) + " Current Possition: " + Queue.OrderBy(x => x.Priority(TimePeriod)).ToList().IndexOf(reqItem) + " Id " + reqItem.Key);

                // remove item from current Queue
                Queue.Remove(reqItem);

                // Call Comunication Agent to Requeue
                CreateAndEnqueue(HubAgent.Instruction.EnqueueWorkItem.Create(reqItem.UpdateStatus(ElementStatus.Created), reqItem.HubAgent));
            }
        }

        private void DoWork()
        {
            if (_itemsInProgess)
            {
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

            DebugMessage("Start With " + item.WorkSchedule.Name);
            _itemsInProgess = true;
            item = item.UpdateStatus(ElementStatus.Processed);


            // TODO: Roll delay here
            var duration = _workTimeGenerator.GetRandomWorkTime(item.WorkSchedule.Duration);

            //Debug.WriteLine("Duration: " + duration + " for " + item.WorkSchedule.Name);

            // TODO !
            // Statistics.UpdateSimulationWorkSchedule(item.Id.ToString(), (int)Context.TimePeriod, duration - 1, this.Machine);

            // get item = ready and lowest priority
            CreateAndEnqueue(Instruction.FinishWork.Create(item, Self), duration);
        }

        private void AcknowledgeProposal(WorkItem workItem)
        {
            if (workItem == null)
                throw new InvalidCastException("Could not Cast Workitem on InstructionSet.ObjectToProcess");

            if (Queue.Any(e => e.Priority(TimePeriod) <= workItem.Priority(TimePeriod)))
            {
                // Get item Latest End.
                var maxItem = Queue.Where(e => e.Priority(TimePeriod) <= workItem.Priority(TimePeriod)).Max(e => e.EstimatedEnd);

                // check if Queuable
                if (maxItem > workItem.EstimatedStart)
                {
                    // reset Agent Status
                    workItem = workItem.UpdateStatus(ElementStatus.Created);
                    SendProposalTo(Sender, workItem);
                    return;
                }
            }

            DebugMessage("AcknowledgeProposal and Enqueued Item: " + workItem.WorkSchedule.Name);
            Queue.Add(workItem);

            // Enqued before another item?
            var position = Queue.OrderBy(x => x.Priority(TimePeriod)).ToList().IndexOf(workItem);
            DebugMessage("Position: " + position + " Priority:" + workItem.Priority(TimePeriod) + " Queue length " + Queue.Count());

            // reorganize Queue if an Element has ben Queued which is More Important.
            if (position + 1 < Queue.Count)
            {
                var toRequeue = Queue.OrderBy(x => x.Priority(TimePeriod)).ToList().GetRange(position + 1, Queue.Count() - position - 1);

                CallToReQueue(toRequeue);

                DebugMessage("New Queue length = " + Queue.Count);
            }


            if (workItem.Status == ElementStatus.Ready)
            {
                // update Processing queue
                UpdateProcessingQueue(workItem);

                // there is at least Something Ready so Start Work
                DoWork();
            }
        }

        private void RequestProposal(WorkItem workItem)
        {
            if (workItem == null)
                throw new InvalidCastException("Could not Cast Workitem on InstructionSet.ObjectToProcess");

            // debug
            DebugMessage("Request for Proposal");
            // Send
            SendProposalTo(Sender, workItem);
        }

        private void SendProposalTo(IActorRef targetAgent, WorkItem workItem)
        {
            long max = 0;
            if (Queue.Any(e => e.Priority(TimePeriod) <= workItem.Priority(TimePeriod)))
            {
                max = Queue.Where(e => e.Priority(TimePeriod) <= workItem.Priority(TimePeriod)).Max(e => e.EstimatedEnd);
            }

            // calculat Proposal.
            var proposal = new Proposal(possibleSchedule: max
                                            , postponed: (max > QueueLength && workItem.Status != ElementStatus.Ready)
                                            , postponedFor: QueueLength
                                            , workItemId: workItem.Key
                                            , resourceAgent: Self);

            // callback 
            CreateAndEnqueue(HubAgent.Instruction.ProposalFromMachine.Create(proposal, targetAgent));
        }

        private void SetHubAgent(IActorRef hubAgent)
        {
            // save Cast to expected object
            _hubAgent = hubAgent;

            // throw if cast failed.
            if (_hubAgent == null)
                throw new ArgumentException("Could not Cast Communication ActorRef from Instruction");

            // Debug Message
            DebugMessage("Successfull Registred Service at : " + _hubAgent.Path.Name);
        }
    }
}
