using System;
using System.Collections.Generic;
using System.Linq;
using Akka.Actor;
using Master40.DB.DataModel;
using Master40.SimulationCore.Agents.HubAgent;
using Master40.SimulationCore.Helper;
using Master40.SimulationImmutables;

namespace Master40.SimulationCore.Agents.Ressource
{
    // Alt: CapacityAgent, ProducerAgent, PotencialFactorAgent, SchedulingAgent, 
    public partial class Resource : Agent
    {
        // public Constructor
        public static Props Props(ActorPaths actorPaths,M_Resource machine, WorkTimeGenerator workTimeGenerator, long time, bool debug, IActorRef principal)
        {
            return Akka.Actor.Props.Create(() => new Resource(actorPaths, machine, workTimeGenerator, time, debug, principal));
        }

        public Resource(ActorPaths actorPaths, M_Resource machine, WorkTimeGenerator workTimeGenerator, long time, bool debug, IActorRef principal) : base(actorPaths, time, debug, principal)
        {
            this.Set(Properties.RESOURCE, machine);
            this.Set(Properties.WORK_TIME_GENERATOR, workTimeGenerator);
            this.Set(Properties.HUB_AGENT_REF, principal);
            //this.Send(Hub.Instruction.AddMachineToHub.Create(new FHubInformation(ResourceType.Machine, machine.MachineGroup.Name, this.Self), principal));
            this.Send(Hub.Instruction.AddMachineToHub.Create(new FHubInformation(ResourceType.Machine, machine.ResourceSetups.First().ResourceSkill.Name, this.Self), principal));
        }

        
        internal void UpdateProcessingQueue(FBucket bucketItem)
        {
            var processingQueue = Get<LimitedQueue<FBucket>>(Properties.PROCESSING_QUEUE);
            var queue = Get<List<FBucket>>(Properties.QUEUE);
            var hub = Get<IActorRef>(Properties.HUB_AGENT_REF);
            if (processingQueue.CapacitiesLeft && bucketItem != null)
            {
                Send(Hub.Instruction.ProductionStarted.Create(bucketItem, hub));
                processingQueue.Enqueue(bucketItem);
                queue.Remove(bucketItem);
            }
        }

        internal void CallToReQueue(List<FBucket> queue, List<FBucket> toRequeue)
        {
            foreach (var reqItem in toRequeue)
            {
                DebugMessage("-> ToRequeue " + reqItem.Priority(TimePeriod) + " Current Possition: " + queue.OrderBy(x => x.Priority(TimePeriod)).ToList().IndexOf(reqItem) + " Id " + reqItem.Key);

                // remove item from current Queue
                queue.Remove(reqItem);

                // Call Comunication Agent to Requeue
                Send(Hub.Instruction.EnqueueBucket.Create(reqItem.UpdateStatus(ElementStatus.Created), reqItem.HubAgent));
            }
        }

        /// <summary>
        /// Starts the next WorkItem
        /// </summary>
        internal void DoWork()
        {
            if (Get<bool>(Properties.ITEMS_IN_PROGRESS))
            {
                DebugMessage("Im still working....");
                return; // still working
            }

            // Dequeue
            var processingQueue = Get<LimitedQueue<FBucket>>(Properties.PROCESSING_QUEUE);
            var item = processingQueue.Dequeue();

            // Wait if nothing More todo.
            if (item == null)
            {
                // No more work 
                DebugMessage("Nothing more Ready in Queue!");
                return;
            }

            
            DebugMessage("Start with Bucket " + item.Key);
            Set(Properties.ITEMS_IN_PROGRESS, true);
            item = item.UpdateStatus(ElementStatus.Processed);

            //Check if Setup
            var resource = this.Get<M_Resource>(Properties.RESOURCE);
            var equippdedResourceTool = Get<M_ResourceTool>(Properties.EQUP_RESOURCETOOL);

            var neededresourcetool = item.Operations.FirstOrDefault().Operation.ResourceSkill.ResourceSetups.Where(x => x.Resource.ResourceId == resource.ResourceId).Select(y => y.ResourceTool) as M_ResourceTool;
            
            if (equippdedResourceTool == neededresourcetool)
            {
                processWorkItemsFromBucket(item);
            }
            else
            {
                startSetupResource(neededresourcetool, item);
            }            
        }

        private void startSetupResource(M_ResourceTool resourceTool, FBucket bucket)
        {
            throw new NotImplementedException();
        }

        internal void processWorkItemsFromBucket(FBucket bucket)
        {


            var workItemsInBucket = bucket.Operations.OrderBy(bucket.Operations.Select(x => x.Priority));

 
            // TODO: Roll delay here
            var workTimeGenerator = Get<WorkTimeGenerator>(Properties.WORK_TIME_GENERATOR);
            var duration = workTimeGenerator.GetRandomWorkTime(item.Operation.Duration);

            //Debug.WriteLine("Duration: " + duration + " for " + item.WorkSchedule.Name);

            // TODO !
            var pub = new UpdateSimulationWork(item.Key.ToString(), duration - 1, (int)this.TimePeriod, this.Name);
            this.Context.System.EventStream.Publish(pub);
            // Statistics.UpdateSimulationWorkSchedule(item.Id.ToString(), (int)Context.TimePeriod, duration - 1, this.Machine);

            // get item = ready and lowest priority
            Send(Instruction.FinishWork.Create(item, Context.Self), duration);

        }

        /// <summary>
        /// Send Proposal to Comunication Client
        /// </summary>
        /// <param name="workItem"></param>
        internal void SendProposalTo(FBucket bucket)
        {
            long max = 0;
            var queue = Get<List<FBucket>>(Properties.QUEUE);
            var queueLength = this.Get<int>(Properties.QUEUE_LENGTH);

            if (queue.Any(e => e.Priority(this.CurrentTime) <= bucket.Priority(this.CurrentTime)))
            {
                max = queue.Where(e => e.Priority(this.CurrentTime) <= bucket.Priority(this.CurrentTime)).Max(e => e.EstimatedEnd);
            }

            // calculat Proposal.
            var proposal = new FProposal(possibleSchedule: max
                                            , postponed: (max > queueLength && bucket.Status != ElementStatus.Ready)
                                            , postponedFor: queueLength
                                            , bucketId: bucket.Key
                                            , resourceAgent: this.Context.Self);

            // callback 
            this.Send(Hub.Instruction.ProposalFromMachine.Create(proposal, this.Context.Sender));
        }


    }
}
