using Master40.SimulationImmutables;
using System;
using System.Collections.Generic;

namespace Master40.SimulationCore.Agents.HubAgent.Buckets.Behaviour
{
    public static class AddWorkItemToBucket
    {
        public static void Action(Hub agent, FWorkItem workItem)
        {
            if (workItem == null)
            {
                throw new InvalidCastException("Could not Cast Workitem on InstructionSet.ObjectToProcess");
            }

            var workItemQueue = agent.Get<List<FWorkItem>>(Hub.Properties.WORK_ITEM_QUEUE);
            var bucketList = agent.Get<List<FBucket>>(Hub.Properties.BUCKETS);

            var localWorkItem = workItemQueue.Find(x => x.Key == workItem.Key);

            // If item is not Already in Queue Add item to Queue
            // // happens i.e. Machine calls to Requeue item.
            if (localWorkItem == null)
            {

                // Set Comunication agent.
                localWorkItem = workItem.UpdateHubAgent(agent.Context.Self);

                // add TO queue
                workItemQueue.Add(localWorkItem);

                //ad to bucket
                agent.findBucketForWorkItem(localWorkItem);

                agent.DebugMessage("Got Item to Add: " + workItem.Operation.Name + " | with status:" + workItem.Status);
            }
            else
            {
                // reset Item.
                agent.DebugMessage("Got Item to Requeue: " + workItem.Operation.Name + " | with status:" + workItem.Status);
                localWorkItem.Proposals.Clear();
            }

        }
    }

}
