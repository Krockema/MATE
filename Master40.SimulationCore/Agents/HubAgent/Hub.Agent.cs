using Akka.Actor;
using Master40.DB.DataModel;
using Master40.SimulationCore.Agents.Ressource;
using Master40.SimulationCore.Helper;
using Master40.SimulationCore.MessageTypes;
using Master40.SimulationImmutables;
using Microsoft.FSharp.Collections;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Master40.SimulationCore.Agents.HubAgent
{
    /// <summary>
    /// Alternative Namen; ResourceAllocation, RessourceGroup, Mediator, Coordinator, Hub
    /// </summary>
    public partial class Hub : Agent
    {
        // public Constructor
        public static Props Props(ActorPaths actorPaths, long time,string skillGroup, bool debug, IActorRef principal)
        {
            return Akka.Actor.Props.Create(() => new Hub(actorPaths, time, skillGroup, debug, principal));
        }

        public Hub(ActorPaths actorPaths, long time, string skillGroup, bool debug, IActorRef principal) : base(actorPaths, time, debug, principal)
        {
            DebugMessage("I'm Alive:" + Context.Self.Path);
            this.Do(BasicInstruction.Initialize.Create(Self, HubBehaviour.Get(skillGroup)));
        }

        internal void findBucketForWorkItem(FWorkItem workItem)
        {
            if (workItem == null)
            {
                throw new InvalidCastException("Could not Cast WorkItem on InstructionSet.ObjectToProcess");
            }

            var bucketList = Get<List<FBucket>>(Properties.BUCKETS);

            //TODO: Do somethine really smart 
            // for now: create bucket for each workItem - lot size 1


            createNewBucket(workItem);

            /*
            foreach (FBucket bucket in bucketList)
            {

                if (bucket.Operations.Count + 1 <= bucket.MaxBucketSize)
                {
                    //Fits to any Bucket do
                    if (true)
                    {
                        addWorkItemToBucket(workItem, bucket);
                    }
                }
                else
                {
                    //Need new Bucket
                    createNewBucket(workItem);
                }

            }
            */
        }

        internal void createNewBucket(FWorkItem workItem)
        {
            if (workItem == null)
                {
                    throw new InvalidCastException("Could not Cast WorkItem on InstructionSet.ObjectToProcess");
                }

            var bucketList = Get<List<FBucket>>(Properties.BUCKETS);

            var newBucket = MessageFactory.ToBucketItem(workItem, TimePeriod);
            bucketList.Add(newBucket);

            DebugMessage("New Bucket: " + newBucket.Key + " for Item: " + workItem.Operation.Name + " created");

            enqueueBucket(newBucket);
        }


        internal void addWorkItemToBucket(FWorkItem workItem, FBucket bucket)
        {
            if (bucket == null)
            {
                throw new InvalidCastException("Could not Cast Bucket on InstructionSet.ObjectToProcess");
            }

            bucket = bucket.AddOperation(workItem);
            bucket.Priority(TimePeriod);
            bucket = bucket.UpdateDueTime;

            DebugMessage("Item: " + workItem.Operation.Name + "added to Bucket: " + bucket.Key);

            enqueueBucket(bucket);

        }

        internal void enqueueBucket(FBucket bucket)
        {
            if (bucket == null)
            {
                throw new InvalidCastException("Could not Cast Bucket on InstructionSet.ObjectToProcess");
            }

            var resourceAgents = Get<Dictionary<IActorRef, string>>(Hub.Properties.RESOURCE_AGENTS);

            foreach (var actorRef in resourceAgents)
            {
                //Only send to Resources for the bucket
                 Send(instruction: Resource.Instruction.RequestProposal.Create(bucket, actorRef.Key));
            }
        }

        internal FBucket getBucketByWorkItemId(int workItemKey)
        {
            var bucketList = Get<List<FBucket>>(Properties.BUCKETS);

            //FBucket bucket = bucketList.Where(b => b.Operations.Where(y => y.Key.Equals(workItemKey));

            FBucket bucket = bucketList.Find(b => b.Operations.Any(c => c.Operation.Id == workItemKey));

            return bucket;
        }

    }
}
