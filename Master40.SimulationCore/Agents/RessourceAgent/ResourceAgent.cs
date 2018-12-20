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
using Master40.SimulationCore.MessageTypes;

namespace Master40.SimulationCore.Agents
{
    // Alt: CapacityAgent, ProducerAgent, PotencialFactorAgent, SchedulingAgent, 
    public partial class ResourceAgent : Agent
    {

        // Agent to register your Services
        /*
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
        */


        // public Constructor
        public static Props Props(ActorPaths actorPaths,Machine machine, WorkTimeGenerator workTimeGenerator, long time, bool debug)
        {
            return Akka.Actor.Props.Create(() => new ResourceAgent(actorPaths, machine, workTimeGenerator, time, debug));
        }

        public ResourceAgent(ActorPaths actorPaths, Machine machine, WorkTimeGenerator workTimeGenerator, long time, bool debug) : base(actorPaths, time, debug)
        {
            this.ValueStore.Add(ResourceBehaviour.MACHINE, machine);
            this.ValueStore.Add(ResourceBehaviour.WORK_TIME_GENERATOR, workTimeGenerator);
        }

        protected override void OnInit(BehaviourSet o)
        {
            o.Object = 
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

        internal void UpdateProcessingQueue(WorkItem workItem)
        {
            var processingQueue = Get<LimitedQueue<WorkItem>>(ResourceBehaviour.PROCESSING_QUEUE);
            var queue = Get<List<WorkItem>>(ResourceBehaviour.QUEUE);
            var hub = Get<IActorRef>(ResourceBehaviour.HUB_AGENT_REF);
            if (processingQueue.CapacitiesLeft && workItem != null)
            {
                if (workItem.WorkSchedule.HierarchyNumber == 10)
                    Send(HubAgent.Instruction.ProductionStarted.Create(workItem, hub));
                processingQueue.Enqueue(workItem);
                queue.Remove(workItem);
            }
        }

        internal void CallToReQueue(List<WorkItem> toRequeue)
        {
            var queue = Get<List<WorkItem>>(ResourceBehaviour.QUEUE);
            foreach (var reqItem in toRequeue)
            {
                DebugMessage("-> ToRequeue " + reqItem.Priority(TimePeriod) + " Current Possition: " + queue.OrderBy(x => x.Priority(TimePeriod)).ToList().IndexOf(reqItem) + " Id " + reqItem.Key);

                // remove item from current Queue
                queue.Remove(reqItem);

                // Call Comunication Agent to Requeue
                CreateAndEnqueue(HubAgent.Instruction.EnqueueWorkItem.Create(reqItem.UpdateStatus(ElementStatus.Created), reqItem.HubAgent));
            }
        }

        internal void DoWork()
        {
            var inProgress = Get<bool>(ResourceBehaviour.ITEMS_IN_PROGRESS);
            if (inProgress)
            {
                DebugMessage("Im still working....");
                return; // still working
            }

            // Dequeue
            var processingQueue = Get<LimitedQueue<WorkItem>>(ResourceBehaviour.PROCESSING_QUEUE);
            var item = processingQueue.Dequeue();


            // Wait if nothing More todo.
            if (item == null)
            {
                // No more work 
                DebugMessage("Nothing more Ready in Queue!");
                return;
            }

            DebugMessage("Start With " + item.WorkSchedule.Name);
            inProgress = true;
            item = item.UpdateStatus(ElementStatus.Processed);


            // TODO: Roll delay here
            var workTimeGenerator = Get<WorkTimeGenerator>(ResourceBehaviour.WORK_TIME_GENERATOR);
            var duration = workTimeGenerator.GetRandomWorkTime(item.WorkSchedule.Duration);

            //Debug.WriteLine("Duration: " + duration + " for " + item.WorkSchedule.Name);

            // TODO !
            // Statistics.UpdateSimulationWorkSchedule(item.Id.ToString(), (int)Context.TimePeriod, duration - 1, this.Machine);

            // get item = ready and lowest priority
            Send(Instruction.FinishWork.Create(item, Context.Self), duration);
        }

        public void AcknowledgeProposal(WorkItem workItem)
        {
            if (workItem == null)
                throw new InvalidCastException("Could not Cast Workitem on InstructionSet.ObjectToProcess");

            var queue = Get<List<WorkItem>>(ResourceBehaviour.QUEUE);
            if (queue.Any(e => e.Priority(TimePeriod) <= workItem.Priority(TimePeriod)))
            {
                // Get item Latest End.
                var maxItem = queue.Where(e => e.Priority(TimePeriod) <= workItem.Priority(TimePeriod)).Max(e => e.EstimatedEnd);

                // check if Queuable
                if (maxItem > workItem.EstimatedStart)
                {
                    // reset Agent Status
                    workItem = workItem.UpdateStatus(ElementStatus.Created);
                    SendProposalTo(this, workItem);
                    return;
                }
            }

            DebugMessage("AcknowledgeProposal and Enqueued Item: " + workItem.WorkSchedule.Name);
            queue.Add(workItem);

            // Enqued before another item?
            var position = queue.OrderBy(x => x.Priority(TimePeriod)).ToList().IndexOf(workItem);
            DebugMessage("Position: " + position + " Priority:" + workItem.Priority(TimePeriod) + " Queue length " + queue.Count());

            // reorganize Queue if an Element has ben Queued which is More Important.
            if (position + 1 < queue.Count)
            {
                var toRequeue = queue.OrderBy(x => x.Priority(TimePeriod)).ToList().GetRange(position + 1, queue.Count() - position - 1);

                CallToReQueue(toRequeue);

                DebugMessage("New Queue length = " + queue.Count);
            }


            if (workItem.Status == ElementStatus.Ready)
            {
                // update Processing queue
                UpdateProcessingQueue(workItem);

                // there is at least Something Ready so Start Work
                DoWork();
            }
        }

        private static void RequestProposal(Agent agent, WorkItem workItem)
        {
            if (workItem == null)
                throw new InvalidCastException("Could not Cast Workitem on InstructionSet.ObjectToProcess");

            // debug
            agent.DebugMessage("Request for Proposal");
            // Send
            ResourceAgent.SendProposalTo(agent, workItem);
        }

        internal static void SendProposalTo(Agent agent, WorkItem workItem)
        {
            long max = 0;
            var queue = agent.Get<List<WorkItem>>(ResourceBehaviour.QUEUE);
            var queueLength = agent.Get<int>(ResourceBehaviour.QUEUE_LENGTH);

            if (queue.Any(e => e.Priority(agent.CurrentTime) <= workItem.Priority(agent.CurrentTime)))
            {
                max = queue.Where(e => e.Priority(agent.CurrentTime) <= workItem.Priority(agent.CurrentTime)).Max(e => e.EstimatedEnd);
            }

            // calculat Proposal.
            var proposal = new Proposal(possibleSchedule: max
                                            , postponed: (max > queueLength && workItem.Status != ElementStatus.Ready)
                                            , postponedFor: queueLength
                                            , workItemId: workItem.Key
                                            , resourceAgent: agent.Context.Self);

            // callback 
            agent.Send(HubAgent.Instruction.ProposalFromMachine.Create(proposal, agent.Context.Sender));
        }

        private static void SetHubAgent(Agent agent, IActorRef hubAgent)
        {
            // save Cast to expected object
            var _hub = hubAgent as IActorRef;
            
            // throw if cast failed.
            if (_hub == null)
                throw new ArgumentException("Could not Cast Communication ActorRef from Instruction");

            // Save to Value Store
            agent.ValueStore.Add(ResourceBehaviour.HUB_AGENT_REF, hubAgent);
            // Debug Message
            agent.DebugMessage("Successfull Registred Service at : " + _hub.Path.Name);
        }
    }
}
