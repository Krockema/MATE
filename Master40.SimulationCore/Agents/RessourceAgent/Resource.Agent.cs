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
                Send(Hub.Instruction.EnqueueBucket.Create(reqItem.UpdateStatus(ElementStatus.Created), reqItem.Operations.FirstOrDefault().HubAgent));
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

            var neededResourceSetup = item.Operations.FirstOrDefault().Operation.ResourceSkill.ResourceSetups.Where(x => x.Resource.ResourceId == resource.ResourceId).Single() as M_ResourceSetup;
            
            if (equippdedResourceTool == neededResourceSetup.ResourceTool)
            {
                processWorkItemsFromBucket(item);
            }
            else
            {
                startSetupResource(neededResourceSetup, item);
            }            
        }

        private void startSetupResource(M_ResourceSetup resourceSetup, FBucket bucket)
        {
            DebugMessage("Do Setup to " + resourceSetup.ResourceTool.Name);

            var pub = new UpdateSimulationWork(bucket.Key.ToString(), resourceSetup.SetupTime - 1, (int)this.TimePeriod, this.Name);
            this.Context.System.EventStream.Publish(pub);

            // get item = ready and lowest priority
            Send(Instruction.FinishSetup.Create(bucket, Context.Self), resourceSetup.SetupTime);

        }

        internal void processWorkItemsFromBucket(FBucket bucket)
        {
           
            var firstWorkItemsInBucket = bucket.Operations.MinimumElement;

            if (firstWorkItemsInBucket == null)
            {
                throw new InvalidCastException("No Item in in bucket to process");
            }

            // TODO: Roll delay here
            var workTimeGenerator = Get<WorkTimeGenerator>(Properties.WORK_TIME_GENERATOR);
            var duration = workTimeGenerator.GetRandomWorkTime(firstWorkItemsInBucket.Operation.Duration);

            bucket.RemoveOperation(firstWorkItemsInBucket);

            var pub = new UpdateSimulationWork(firstWorkItemsInBucket.Key.ToString(), duration - 1, (int)this.TimePeriod, this.Name);
            this.Context.System.EventStream.Publish(pub);

            bucket.RemoveOperation(firstWorkItemsInBucket);

            Send(Instruction.FinishWorkItem.Create(firstWorkItemsInBucket, Context.Self), duration);

            if(bucket.Operations.IsEmpty == false) { 
                Send(Instruction.StartWorkWithNextItem.Create(bucket, Context.Self), duration);
            }
            else
            {
                Send(Instruction.FinishWorkBucket.Create(bucket, Context.Self));
            }
        }

        /// <summary>
        /// Send Proposal to Comunication Client
        /// </summary>
        /// <param name="workItem"></param>
        internal void SendProposalTo(FBucket bucket)
        {

            var _bucket = bucket;

            if (bucket.GetType() != typeof(FBucket))
            {
                throw new InvalidCastException("Could not Cast >Bucket< on InstructionSet.ObjectToProcess");
            }

            long max = 0;

            var limitedqueue = this.Get<LimitedQueue<FBucket>>(Properties.PROCESSING_QUEUE);
            var queue = this.Get<List<FBucket>>(Properties.QUEUE);
            
            var queueLength = this.Get<int>(Properties.QUEUE_LENGTH);

            if (queue.Any(e => e.Priority(this.CurrentTime) <= bucket.Priority(this.CurrentTime)))
            {
                max = queue.Where(e => e.Priority(this.CurrentTime) <= bucket.Priority(this.CurrentTime)).Max(e => e.EstimatedEnd);
            }

            // calculat Proposal.
            var proposal = new FProposal(possibleSchedule: max
                                            ,workItemId: bucket.Key
                                            , postponed: (max > queueLength && bucket.Status != ElementStatus.Ready)
                                            , postponedFor: queueLength
                                            , bucketId: bucket.Key
                                            , resourceAgent: this.Context.Self);

            // callback 
            this.Send(Hub.Instruction.ProposalFromMachine.Create(proposal, this.Context.Sender));
        }


    }
}
