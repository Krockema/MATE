using Akka.Actor;
using Master40.DB.Data.Context;
using Master40.DB.GanttPlanModel;
using Master40.DB.Nominal;
using Master40.SimulationCore.Agents.HubAgent.Types.Central;
using Master40.SimulationCore.Agents.ResourceAgent;
using Master40.SimulationCore.Helper.DistributionProvider;
using Master40.Tools.Connectoren.Ganttplan;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using Master40.DB.Data.Helper;
using Master40.Tools.ExtensionMethods;
using static FCentralActivities;
using static FCentralResourceRegistrations;
using System;
using Master40.SimulationCore.Agents.StorageAgent;
using static FCentralProvideMaterials;
using static FCentralStockPostings;

namespace Master40.SimulationCore.Agents.HubAgent.Behaviour
{
    class Central : SimulationCore.Types.Behaviour
    {
        private Dictionary<int, FCentralActivity> _scheduledActivities { get; } = new Dictionary<int, FCentralActivity>();
        private string _dbConnectionStringGanttPlan { get; }
        private string _dbConnectionStringMaster { get; }
        private WorkTimeGenerator _workTimeGenerator { get; }
        private PlanManager _planManager { get; } = new PlanManager();
        private ResourceManager _resourceManager { get; } = new ResourceManager();
        private ActivityManager _activityManager { get; } = new ActivityManager();
        private ConfirmationManager _confirmationManager { get; }

        public Central(string dbConnectionStringGanttPlan, string dbConnectionStringMaster, WorkTimeGenerator workTimeGenerator, SimulationType simulationType = SimulationType.Default) : base(childMaker: null, simulationType: simulationType)
        {
            _workTimeGenerator = workTimeGenerator;
            _dbConnectionStringGanttPlan = dbConnectionStringGanttPlan;
            _dbConnectionStringMaster = dbConnectionStringMaster;
            _confirmationManager = new ConfirmationManager(dbConnectionStringGanttPlan);
        }

            public override bool Action(object message)
            {
                switch (message)
                {
                    case Hub.Instruction.Central.AddResourceToHub msg: AddResourceToHub(msg.GetResourceRegistration); break;
                    case Hub.Instruction.Central.LoadProductionOrders msg: LoadProductionOrders(msg.GetInboxActorRef); break;
                    case Hub.Instruction.Central.StartActivities msg: StartActivities(); break;
                    case Hub.Instruction.Central.ScheduleActivity msg: ScheduleActivity(msg.GetActivity); break;
                    case Hub.Instruction.Central.ActivityFinish msg: FinishActivity(msg.GetObjectFromMessage); break;
                    default: return false;
                }
                return true;
            }

        private void AddResourceToHub(FCentralResourceRegistration resourceRegistration)
        {
            var resourceDefintion = new ResourceDefinition(resourceRegistration.ResourceName, resourceRegistration.ResourceId, resourceRegistration.ResourceActorRef, resourceRegistration.ResourceGroupId, resourceRegistration.ResourceType);
            _resourceManager.Add(resourceDefintion);
        }

        private void LoadProductionOrders(IActorRef inboxActorRef)
        {
            //update model planning time
            using (var localGanttPlanDbContext = GanttPlanDBContext.GetContext(_dbConnectionStringGanttPlan))
            {
                var modelparameter = localGanttPlanDbContext.GptblModelparameter.FirstOrDefault();
                if (modelparameter != null)
                {
                    modelparameter.ActualTime = Agent.CurrentTime.ToDateTime();
                    localGanttPlanDbContext.SaveChanges();
                }
            }

            System.Diagnostics.Debug.WriteLine("Start GanttPlan");
            GanttPlanOptRunner.RunOptAndExport("Continuous");
            System.Diagnostics.Debug.WriteLine("Finish GanttPlan");

            using (var localGanttPlanDbContext = GanttPlanDBContext.GetContext(_dbConnectionStringGanttPlan))
            {
                foreach (var resourceState in _resourceManager.resourceStateList)
                {
                    // put to an sorted Queue on each Resource
                    resourceState.ActivityQueue = new Queue<GptblProductionorderOperationActivityResourceInterval>(
                        localGanttPlanDbContext.GptblProductionorderOperationActivityResourceInterval
                            .Include(x => x.ProductionorderOperationActivityResource)
                                .ThenInclude(x => x.ProductionorderOperationActivity)
                                    .ThenInclude(x => x.ProductionorderOperationActivityMaterialrelation)
                            .Include(x => x.ProductionorderOperationActivityResource)
                                .ThenInclude(x => x.ProductionorderOperationActivity)
                                    .ThenInclude(x => x.Productionorder)
                            .Include(x => x.ProductionorderOperationActivityResource)
                                .ThenInclude(x => x.ProductionorderOperationActivity)
                                    .ThenInclude(x => x.ProductionorderOperationActivityResources)
                            .Where(x => x.ResourceId.Equals(resourceState.ResourceDefinition.Id) 
                                    && x.IntervalAllocationType.Equals(1))
                            .OrderBy(x => x.DateFrom)
                            .ToList());
                } // filter Done and in Progress?
            }

            //delete orders to avoid sync
            using (var masterDBContext = MasterDBContext.GetContext(_dbConnectionStringMaster))
            {
                masterDBContext.CustomerOrders.RemoveRange(masterDBContext.CustomerOrders);
                masterDBContext.CustomerOrderParts.RemoveRange(masterDBContext.CustomerOrderParts);
                masterDBContext.SaveChanges();
            }

            _confirmationManager.TransferConfirmations();

            _planManager.IncrementPlaningNumber();
            _scheduledActivities.Clear();

            StartActivities();

            inboxActorRef.Tell("GanttPlan finished!", this.Agent.Context.Self);
           
        }

        private void StartActivities()
        {
            foreach (var resourceState in _resourceManager.resourceStateList)
            {
                //Skip if resource is working
                if(resourceState.IsWorking) continue;

                if (resourceState.ActivityQueue.IsNull() || resourceState.ActivityQueue.Count < 1) continue;

                var interval = resourceState.ActivityQueue.Peek();

                //CheckMaterial
                
                var featureActivity = new FCentralActivity(resourceState.ResourceDefinition.Id
                    , interval.ProductionorderId
                    , interval.OperationId
                    , interval.ActivityId
                    , interval.ConvertedDateTo - interval.ConvertedDateFrom
                    , interval.ActivityId.ToString()
                    , _planManager.PlanVersion
                    , Agent.Context.Self); // may not required.
                
                // Feature Activity
                if (!interval.ConvertedDateFrom.Equals(Agent.CurrentTime))
                {
                    // only schedule activities that have not been scheduled
                    if(_scheduledActivities.TryAdd(interval.ActivityId, featureActivity))
                    {
                        Agent.Send(instruction: Hub.Instruction.Central.ScheduleActivity.Create(featureActivity, Agent.Context.Self)
                                            , interval.ConvertedDateFrom);
                    }
                }
                else
                {   // Activity is scheduled for now
                    TryStartActivity(interval, featureActivity);
                }
            }
        }

        /// <summary>
        /// Check if Feature is still active.
        /// 
        /// </summary>
        /// <param name="featureActivity"></param>
        private void ScheduleActivity(FCentralActivity featureActivity)
        {
            if (_scheduledActivities.ContainsKey(featureActivity.ActivityId))
            {
                var resource = _resourceManager.resourceStateList.Single(x => x.ResourceDefinition.Id == featureActivity.ResourceId);
                if (NextActivityIsEqual(resource, featureActivity.ProductionOrderId, featureActivity.OperationId, featureActivity.ActivityId))
                {
                    _scheduledActivities.Remove(featureActivity.ActivityId);
                    TryStartActivity(resource.ActivityQueue.Dequeue(), featureActivity);
                }
            }
        }

        private void TryStartActivity(GptblProductionorderOperationActivityResourceInterval interval,FCentralActivity fActivity)
        {
            // not sure if this works or if we just loop through resources and check even activities
            var resourcesForActivity = interval.ProductionorderOperationActivityResource
                                                .ProductionorderOperationActivity
                                                .ProductionorderOperationActivityResources;

            var requiredResources = new List<ResourceState>();
            //activity can be ignored as long any resource is working -> after finish work of the resource it will trigger anyways
            foreach (var resourceForActivity in resourcesForActivity)
            {
                var resourceState = _resourceManager.resourceStateList.Single(x => x.ResourceDefinition.Id == resourceForActivity.ResourceId);
                if (_resourceManager.ResourceIsWorking(resourceForActivity.ResourceId)
                    || !NextActivityIsEqual(resourceState, interval.ProductionorderId, interval.OperationId, interval.ActivityId))
                {
                    Agent.DebugMessage($"{resourceState.ResourceDefinition.Name} has work or different activity. Stop TryStartActivity!");
                    return;
                }
                requiredResources.Add(resourceState);
            }

            //Check if all preconditions are fullfilled
            var activity = interval.ProductionorderOperationActivityResource.ProductionorderOperationActivity;
            if (!_activityManager.HasPreconditionsFullfilled(activity))
                return;

            StartActivity(requiredResources: requiredResources
                , interval: interval
                , featureActivity: fActivity);
        }

        private bool NextActivityIsEqual(ResourceState resourceState, string productionOrderId, string operationId, int activityId)
        {
            var nextActivity = resourceState.ActivityQueue.Peek();
            return nextActivity != null
                   && nextActivity.ProductionorderId == productionOrderId
                   && nextActivity.OperationId == operationId
                   && nextActivity.ActivityId == activityId;
        }

        
        private void StartActivity(List<ResourceState> requiredResources
            , GptblProductionorderOperationActivityResourceInterval interval
            , FCentralActivity featureActivity)
        {
            var activity = interval.ProductionorderOperationActivityResource.ProductionorderOperationActivity;

            WithdrawalMaterial(activity);

            foreach (var resourceState in requiredResources)
            {

                var fCentralActivity = new FCentralActivity(resourceId: resourceState.ResourceDefinition.Id
                                                    ,productionOrderId: featureActivity.ProductionOrderId
                                                          ,operationId:featureActivity.OperationId
                                                    ,activityId:featureActivity.ActivityId
                                                    ,duration:featureActivity.Duration,name:featureActivity.Name
                                                    ,ganttPlanningInterval:featureActivity.GanttPlanningInterval
                                                    ,hub:featureActivity.Hub);

                var startActivityInstruction = Resource.Instruction.Central
                                                .ActivityStart.Create(activity: fCentralActivity
                                                                       , target: resourceState.ResourceDefinition.AgentRef);
                resourceState.ActivityQueue.Dequeue();
                _activityManager.AddOrUpdateActivity(activity, resourceState.ResourceDefinition);
                resourceState.StartActivityAtResource(activity);
                Agent.Send(startActivityInstruction);
            }

        }
        
        public void FinishActivity(FCentralActivity fActivity)
        {
            // Get Resource
            System.Diagnostics.Debug.WriteLine("Finish Activity {0} with Duration: {1} from {2}", fActivity.Name , fActivity.Duration, Agent.Sender.ToString());

            var activity = _resourceManager.GetCurrentActivity(fActivity.ResourceId);
           
            // Add Confirmation
            _activityManager.FinishActivityForResource(activity, fActivity.ResourceId);

            if (_activityManager.ActivityIsFinished(activity))
            {
                //Check if productionorder finished

                _confirmationManager.AddConfirmations(activity, GanttState.Finished);

                ProductionOrderFinishCheck(activity);
            }

            // Finish ResourceState
            _resourceManager.FinishActivityAtResource(fActivity.ResourceId);

            //Check If new activities are available
            StartActivities();
        }

        private void ProductionOrderFinishCheck(GptblProductionorderOperationActivity activity )
        {
            var productionOrder = activity.Productionorder;

            foreach (var activityOfProductionOrder in productionOrder.ProductionorderOperationActivities)
            {
                var prodactivity = _activityManager.Activities.SingleOrDefault(x => x.Activity.Equals(activity));

                if (prodactivity == null || !prodactivity.ActivityIsFinished)
                {
                    return;
                }
            }

            Agent.DebugMessage($"All resources for {activity.GetActivityName()} returned!");

            // Insert Material

            var storagePosting = new FCentralStockPosting(productionOrder.MaterialId,(double)productionOrder.QuantityNet);
            Agent.Send(DirectoryAgent.Directory.Instruction.Central.InsertMaterial.Create(storagePosting, Agent.ActorPaths.StorageDirectory.Ref));

        }


        /// <summary>
        /// Provide Material
        /// </summary>
        private void WithdrawalMaterial(GptblProductionorderOperationActivity activity)
        {

            var materialId = string.Empty;

            foreach (var material in activity.ProductionorderOperationActivityMaterialrelation)
            {

                switch (material.MaterialrelationType)
                {
                    case 2:
                        //search for the materialId of the productionorder
                        var materialActivity = _activityManager.GetActivity(material.ChildId,
                            material.ChildOperationId,
                            material.ChildActivityId);
                        break;
                    case 8:
                        materialId = material.ChildId;
                        break;
                    default:
                        break;
                }
                var stockPosting = new FCentralStockPostings.FCentralStockPosting(materialId: materialId, (double)material.Quantity);
                Agent.Send(DirectoryAgent.Directory.Instruction.Central.WithdrawMaterial.Create(stockPosting,Agent.ActorPaths.StorageDirectory.Ref));

            }
        }

        public override bool AfterInit()
        {
            Agent.Send(Hub.Instruction.Central.StartActivities.Create(Agent.Context.Self), 1);
            return true;
        }

    }
}
