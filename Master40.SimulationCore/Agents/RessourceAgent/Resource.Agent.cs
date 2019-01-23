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
using static Master40.SimulationCore.Agents.Resource;
using static Master40.SimulationCore.Agents.Resource.Properties;

namespace Master40.SimulationCore.Agents
{
    // Alt: CapacityAgent, ProducerAgent, PotencialFactorAgent, SchedulingAgent, 
    public partial class Resource : Agent
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
        public static Props Props(ActorPaths actorPaths,Machine machine, WorkTimeGenerator workTimeGenerator, long time, bool debug, IActorRef principal)
        {
            return Akka.Actor.Props.Create(() => new Resource(actorPaths, machine, workTimeGenerator, time, debug, principal));
        }

        public Resource(ActorPaths actorPaths, Machine machine, WorkTimeGenerator workTimeGenerator, long time, bool debug, IActorRef principal) : base(actorPaths, time, debug, principal)
        {
            this.ValueStore.Add(MACHINE, machine);
            this.ValueStore.Add(WORK_TIME_GENERATOR, workTimeGenerator);
        }
        
        // protected override void Do(object o)
        // {
        //     switch (o)
        //     {

        //     }
        // }
        //

        internal void UpdateProcessingQueue(WorkItem workItem)
        {
            var processingQueue = Get<LimitedQueue<WorkItem>>(PROCESSING_QUEUE);
            var queue = Get<List<WorkItem>>(QUEUE);
            var hub = Get<IActorRef>(HUB_AGENT_REF);
            if (processingQueue.CapacitiesLeft && workItem != null)
            {
                if (workItem.WorkSchedule.HierarchyNumber == 10)
                    Send(Hub.Instruction.ProductionStarted.Create(workItem, hub));
                processingQueue.Enqueue(workItem);
                queue.Remove(workItem);
            }
        }

        internal void CallToReQueue(List<WorkItem> toRequeue)
        {
            var queue = Get<List<WorkItem>>(QUEUE);
            foreach (var reqItem in toRequeue)
            {
                DebugMessage("-> ToRequeue " + reqItem.Priority(TimePeriod) + " Current Possition: " + queue.OrderBy(x => x.Priority(TimePeriod)).ToList().IndexOf(reqItem) + " Id " + reqItem.Key);

                // remove item from current Queue
                queue.Remove(reqItem);

                // Call Comunication Agent to Requeue
                Send(Hub.Instruction.EnqueueWorkItem.Create(reqItem.UpdateStatus(ElementStatus.Created), reqItem.HubAgent));
            }
        }

        internal void DoWork()
        {
            var inProgress = Get<bool>(ITEMS_IN_PROGRESS);
            if (inProgress)
            {
                DebugMessage("Im still working....");
                return; // still working
            }

            // Dequeue
            var processingQueue = Get<LimitedQueue<WorkItem>>(PROCESSING_QUEUE);
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
            var workTimeGenerator = Get<WorkTimeGenerator>(WORK_TIME_GENERATOR);
            var duration = workTimeGenerator.GetRandomWorkTime(item.WorkSchedule.Duration);

            //Debug.WriteLine("Duration: " + duration + " for " + item.WorkSchedule.Name);

            // TODO !
            // Statistics.UpdateSimulationWorkSchedule(item.Id.ToString(), (int)Context.TimePeriod, duration - 1, this.Machine);

            // get item = ready and lowest priority
            Send(Instruction.FinishWork.Create(item, Context.Self), duration);
        }

        internal void SendProposalTo(WorkItem workItem)
        {
            long max = 0;
            var queue = this.Get<List<WorkItem>>(QUEUE);
            var queueLength = this.Get<int>(QUEUE_LENGTH);

            if (queue.Any(e => e.Priority(this.CurrentTime) <= workItem.Priority(this.CurrentTime)))
            {
                max = queue.Where(e => e.Priority(this.CurrentTime) <= workItem.Priority(this.CurrentTime)).Max(e => e.EstimatedEnd);
            }

            // calculat Proposal.
            var proposal = new Proposal(possibleSchedule: max
                                            , postponed: (max > queueLength && workItem.Status != ElementStatus.Ready)
                                            , postponedFor: queueLength
                                            , workItemId: workItem.Key
                                            , resourceAgent: this.Context.Self);

            // callback 
            this.Send(Hub.Instruction.ProposalFromMachine.Create(proposal, this.Context.Sender));
        }


    }
}
