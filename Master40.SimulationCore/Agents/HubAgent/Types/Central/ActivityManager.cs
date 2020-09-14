using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Master40.DB.GanttPlanModel;

namespace Master40.SimulationCore.Agents.HubAgent.Types.Central
{
    /// <summary>
    /// Trackes Activities and Confirmations
    /// Confirmations: general and foreach resource
    /// Confirmations States: Start, Partial, Finished
    /// </summary>
    public class ActivityManager
    {

        public List<ActivityState> Activities = new List<ActivityState>();

        public void AddOrUpdateActivity(GptblProductionorderOperationActivity activity, ResourceDefinition resourceDefinition)
        {
            var currentActivityState = Activities.SingleOrDefault(x => x.Activity.Equals(activity));

            if (currentActivityState == null)
            {
                currentActivityState = new ActivityState(activity, resourceDefinition);
                Activities.Add(currentActivityState);
                return;
            }

            currentActivityState.AddResource(resourceDefinition);

        }

        public void FinishActivityForResource(GptblProductionorderOperationActivity activity, string resourceId)
        {
            Activities.Single(x => x.Activity.Equals(activity)).FinishForResource(resourceId);
        }

        public bool ActivityIsFinished(GptblProductionorderOperationActivity activity)
        {
            return Activities.Single(x => x.Activity.Equals(activity)).ActivityIsFinished;
        }

        public ActivityState GetActivityState(string productionOrderId, string operationId,
            int activityId)
        {
            return Activities.SingleOrDefault(x => x.Activity.ProductionorderId.Equals(productionOrderId)
                                                                        && x.Activity.OperationId.Equals(operationId)
                                                                        && x.Activity.ActivityId.Equals(activityId));
            
        }
        public GptblProductionorderOperationActivity GetActivity(string productionOrderId, string operationId,
            int activityId)
        {
            return Activities.SingleOrDefault(x => x.Activity.ProductionorderId.Equals(productionOrderId)
                                                   && x.Activity.OperationId.Equals(operationId)
                                                   && x.Activity.ActivityId.Equals(activityId)).Activity;

        }

        internal bool HasPreconditionsFullfilled(GptblProductionorderOperationActivity activity)
        {
            foreach (var requiredPrecondition in activity.ProductionorderOperationActivityMaterialrelation)
            {

                switch (requiredPrecondition.MaterialrelationType)
                {
                    //ProductionOrder
                    case 2:
                        if (!Activities.Exists(x => x.Activity.Equals(activity))
                            || !ActivityIsFinished(activity))
                        {
                            //at least one ProductionOrder is not ready
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

            return true;
        }
    }
}
