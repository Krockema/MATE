using System;
using System.Collections.Generic;
using System.Linq;
using Mate.DataCore.GanttPlan.GanttPlanModel;
using Mate.DataCore.Nominal.Model;

namespace Mate.Production.Core.Agents.HubAgent.Types.Central
{
    /// <summary>
    /// Trackes Activities and Confirmations
    /// Confirmations: general and foreach resource
    /// Confirmations States: Start, Partial, Finished
    /// </summary>
    public class ActivityManager
    {

        public List<ActivityState> Activities = new List<ActivityState>();

        public bool ActivityAlreadyFinished(string key)
        {
            var activity = Activities.FirstOrDefault(x => x.Activity.GetKey.Equals(key));

            if (activity == null)
                return false;
            
            return activity.ActivityIsFinished();

        }

        public void AddOrUpdateActivity(GptblProductionorderOperationActivity activity, ResourceDefinition resourceDefinition, int planVersion)
        {
            var currentActivityState = Activities.SingleOrDefault(x => x.Activity.GetKey.Equals(activity.GetKey));

            //if this is the first activity
            if (currentActivityState == null)
            {
                currentActivityState = new ActivityState(activity, resourceDefinition, planVersion);
                Activities.Add(currentActivityState);
                return;
            }
            
            currentActivityState.AddResource(activity, resourceDefinition, planVersion);

        }

        public void FinishActivityForResource(GptblProductionorderOperationActivity activity, int resourceId)
        {
            Activities.SingleOrDefault(x => x.Activity.GetKey.Equals(activity.GetKey))?.FinishForResource(resourceId);
        }

        public bool ActivityIsFinished(string activityKey)
        {
            return Activities.Single(x => x.Activity.GetKey.Equals(activityKey)).ActivityIsFinished();
        }

        public GptblProductionorderOperationActivity GetActivity(string activityKey)
        {
            return Activities.SingleOrDefault(x => x.Activity.GetKey.Equals(activityKey))?.Activity;

        }

        internal bool HasPreconditionsFullfilled(GptblProductionorderOperationActivity activity, List<ResourceState> resourceStates)
        {
            //TODO: Dangerous!
            int operationId = int.Parse(activity.OperationId);
            var allOperationIds = activity.Productionorder.ProductionorderOperationActivities.Select(x => int.Parse(x.OperationId)).Distinct().ToList();

            if (operationId != allOperationIds.Min())
            {
                allOperationIds.Sort();
                int nextLowest = allOperationIds.LastOrDefault(x => x < operationId);

                var predecessor = Activities.SingleOrDefault(x =>
                    x.Activity.GetKey.Equals(activity.ProductionorderId + "|" + nextLowest.ToString() + "|" + "3"));

                if (predecessor == null || !predecessor.ActivityIsFinished())
                {
                    return false;
                }

            }

            // check if Production Precondition is fulfilled to start setup.
            if (activity.ActivityType.Equals(2))
            {
                var activityQueue = resourceStates.Single(x => x.ResourceDefinition.ResourceType == ResourceType.Workcenter).ActivityQueue;
                activity = activityQueue.Single(x => x.GetKey.Equals(activity.ProductionorderId + "|" + activity.OperationId + "|" + "3"))
                                            .ProductionorderOperationActivityResource
                                                .ProductionorderOperationActivity;
            }

            foreach (var requiredPrecondition in activity.ProductionorderOperationActivityMaterialrelation)
            {

                switch (requiredPrecondition.MaterialrelationType)
                {
                    //ProductionOrder
                    case 2:

                        System.Diagnostics.Debug.WriteLine(
                            $"{activity.ProductionorderId}|{activity.OperationId}|{activity.ActivityId} require {requiredPrecondition.ChildId}|{requiredPrecondition.ChildOperationId}|{requiredPrecondition.ChildActivityId} ");
                      

                        if (!Activities.Exists(x => x.Activity.GetKey == requiredPrecondition.GetChildKey)
                            || !ActivityIsFinished(requiredPrecondition.GetChildKey))
                        {
                            System.Diagnostics.Debug.WriteLine(
                                $"Precondition {requiredPrecondition.ChildId}|{requiredPrecondition.ChildOperationId}|{requiredPrecondition.ChildActivityId} for {activity.ProductionorderId}|{activity.OperationId}|{activity.ActivityId} is not fulfilled");

                            return false;
                        }

                        break;
                    case 8:
                        // ingore buy meterials for now
                        break;
                    default:
                        System.Diagnostics.Debug.WriteLine("Materialtype does not exits!");
                        break;
                }

            }

            System.Diagnostics.Debug.WriteLine(
                $"All preconditions for {activity.ProductionorderId}|{activity.OperationId}|{activity.ActivityId} fulfilled");

            return true;
        }
    }
}
