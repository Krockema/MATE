using Akka.Actor;
using Master40.DB.Data.Context;
using Master40.DB.Data.Helper;
using Master40.DB.GanttPlanModel;
using Master40.DB.Nominal;
using Master40.SimulationCore.Agents.HubAgent.Types.Central;
using Master40.SimulationCore.Agents.ResourceAgent;
using Master40.SimulationCore.Helper;
using Master40.SimulationCore.Helper.DistributionProvider;
using Master40.Tools.Connectoren.Ganttplan;
using Master40.Tools.ExtensionMethods;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using static FCentralActivities;
using static FCentralResourceRegistrations;
using static FCentralStockPostings;
using static FCreateSimulationJobs;

namespace Master40.SimulationCore.Agents.HubAgent.Behaviour
{
    class Central : SimulationCore.Types.Behaviour
    {
        private Dictionary<string, FCentralActivity> _scheduledActivities { get; } = new Dictionary<string, FCentralActivity>();
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
            _confirmationManager.TransferConfirmations();
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
                                        && x.IntervalAllocationType.Equals(1) 
                                        && (x.ProductionorderOperationActivityResource.ProductionorderOperationActivity.Status != (int)GanttActivityState.Finished 
                                            && x.ProductionorderOperationActivityResource.ProductionorderOperationActivity.Status != (int)GanttActivityState.Started))
                            .OrderBy(x => x.DateFrom)
                            .ToList());
                } // filter Done and in Progress?
            }

            /*using (var localGanttPlanDbContext = GanttPlanDBContext.GetContext(_dbConnectionStringGanttPlan))
            {
                _activityManager.UpdateActvityIds(localGanttPlanDbContext.);
            }*/

            //delete orders to avoid sync
            using (var masterDBContext = MasterDBContext.GetContext(_dbConnectionStringMaster))
            {
                masterDBContext.CustomerOrders.RemoveRange(masterDBContext.CustomerOrders);
                masterDBContext.CustomerOrderParts.RemoveRange(masterDBContext.CustomerOrderParts);
                masterDBContext.SaveChanges();
            }

            
            Agent.DebugMessage($"GanttPlanning number {_planManager.PlanVersion} incremented {_planManager.IncrementPlaningNumber()}");
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
                    , interval.ProductionorderOperationActivityResource.ProductionorderOperationActivity.Name
                    , _planManager.PlanVersion
                    , Agent.Context.Self
                    , string.Empty
                    , interval.ActivityId.Equals(2) ? JobType.SETUP : JobType.OPERATION); // may not required.
                
                // Feature Activity
                if (interval.ConvertedDateFrom > Agent.CurrentTime)
                {
                   // only schedule activities that have not been scheduled
                    if(_scheduledActivities.TryAdd(featureActivity.Key, featureActivity))
                    {   
                        var waitFor = interval.ConvertedDateFrom - Agent.CurrentTime;
                        Agent.DebugMessage($"{featureActivity.Key} has been scheduled to {Agent.CurrentTime + waitFor} as planning interval {_planManager.PlanVersion}");
                        Agent.Send(instruction: Hub.Instruction.Central.ScheduleActivity.Create(featureActivity, Agent.Context.Self)
                                            , waitFor);
                    }
                }
                else
                {
                    if (interval.ConvertedDateFrom < Agent.CurrentTime)
                    {
                        Agent.DebugMessage($"Activity {featureActivity.Key} at {resourceState.ResourceDefinition.Name} is delayed {interval.ConvertedDateFrom}");
                    }
                    
                    // Activity is scheduled for now
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
            if (featureActivity.GanttPlanningInterval < _planManager.PlanVersion)
            {
                Agent.DebugMessage($"{featureActivity.Key} has new schedule from more recent planning interval {featureActivity.GanttPlanningInterval}");
                return;
            }

            if (_activityManager.Activities.Any(x => x.Activity.GetKey.Equals(featureActivity.Key)))
            {
                Agent.DebugMessage($"Actvity {featureActivity.Key} already in progress");
                return;
            }

            //Agent.DebugMessage($"TryStart scheduled activity {featureActivity.Key}");
            if (_scheduledActivities.ContainsKey(featureActivity.Key))
            {

                var resource = _resourceManager.resourceStateList.Single(x => x.ResourceDefinition.Id == featureActivity.ResourceId);
                if (NextActivityIsEqual(resource, featureActivity.Key))
                {
                    //Agent.DebugMessage($"Activity {featureActivity.Key} is equal start now after been scheduled by {featureActivity.ResourceId}");
                    _scheduledActivities.Remove(featureActivity.Key);
                    TryStartActivity(resource.ActivityQueue.Peek(), featureActivity);
                }
            }
        }

        private void TryStartActivity(GptblProductionorderOperationActivityResourceInterval interval,FCentralActivity fActivity)
        {
            // not sure if this works or if we just loop through resources and check even activities
            var resourcesForActivity = interval.ProductionorderOperationActivityResource
                                                .ProductionorderOperationActivity
                                                .ProductionorderOperationActivityResources;

            var resource = _resourceManager.resourceStateList.Single(x => x.ResourceDefinition.Id.Equals(fActivity.ResourceId));

            //Agent.DebugMessage($"{resource.ResourceDefinition.Name} try start activity {fActivity.Key}!");

            var requiredResources = new List<ResourceState>();
            var resourceCount = 0;
            //activity can be ignored as long any resource is working -> after finish work of the resource it will trigger anyways
            foreach (var resourceForActivity in resourcesForActivity)
            {
                var resourceState = _resourceManager.resourceStateList.Single(x => x.ResourceDefinition.Id == resourceForActivity.ResourceId);

                //Agent.DebugMessage($"Try to start activity {fActivity.Key} at {resourceState.ResourceDefinition.Name}");
                if (_resourceManager.ResourceIsWorking(resourceForActivity.ResourceId))
                {
                    //Agent.DebugMessage($"{resourceState.ResourceDefinition.Name} has current work{resourceState.GetCurrentProductionOperationActivity}. Stop TryStartActivity!");
                    return;
                }

                if (!NextActivityIsEqual(resourceState, fActivity.Key))
                {
                    var nextActivity = resourceState.ActivityQueue.Peek();
                    //Agent.DebugMessage($"{resourceState.ResourceDefinition.Name} has different work next {nextActivity.ProductionorderId}|{nextActivity.OperationId}|{nextActivity.ActivityId}. Stop TryStartActivity!");
                    return;

                }

                resourceCount++;
                Agent.DebugMessage($"{resourceCount} of {resourcesForActivity.Count} Resource for Activity {fActivity.Key} are ready");
                requiredResources.Add(resourceState);
            }

            //Check if all preconditions are fullfilled
            var activity = interval.ProductionorderOperationActivityResource.ProductionorderOperationActivity;
            
            if (!_activityManager.HasPreconditionsFullfilled(activity, requiredResources))
            {
                Agent.DebugMessage($"Preconditions for {fActivity.Key} are not fulfilled!");
                return;

            }

            StartActivity(requiredResources: requiredResources
                                , interval: interval
                                , featureActivity: fActivity);
        }

        private bool NextActivityIsEqual(ResourceState resourceState, string activityKey)
        {
            var nextActivity = resourceState.ActivityQueue.Peek();
            if (nextActivity == null)
            {
                Agent.DebugMessage($"No next activity for {resourceState.ResourceDefinition.Name} in resource activity queue");
                return false;
            }
            Agent.DebugMessage($"Next activity is {nextActivity.GetKey} but should be {activityKey} ");
            return nextActivity.GetKey == activityKey;
        }

        
        private void StartActivity(List<ResourceState> requiredResources
            , GptblProductionorderOperationActivityResourceInterval interval
            , FCentralActivity featureActivity)
        {
            if (_scheduledActivities.ContainsKey(featureActivity.Key))
            {

                Agent.DebugMessage($"Activity {featureActivity.Key} removed from _scheduledActivities");
                _scheduledActivities.Remove(featureActivity.Key);
            }

            Agent.DebugMessage($"Start activity {featureActivity.ProductionOrderId}|{featureActivity.OperationId}|{featureActivity.ActivityId}!");

            var activity = interval.ProductionorderOperationActivityResource.ProductionorderOperationActivity;

            WithdrawalMaterial(activity);

            _confirmationManager.AddConfirmations(activity, GanttConfirmationState.Started);


            //Capability
            var capability = requiredResources.SingleOrDefault(x => x.ResourceDefinition.ResourceType.Equals(5))?.ResourceDefinition.Name;
            if (capability == null)
            {
                throw new Exception($"No tool (capability) exits. There should be exactly one tool.");
            }

            var randomizedDuration = _workTimeGenerator.GetRandomWorkTime(featureActivity.Duration);

            CreateSimulationJob(activity, Agent.CurrentTime, randomizedDuration, capability);

            foreach (var resourceState in requiredResources)
            {

                var fCentralActivity = new FCentralActivity(resourceId: resourceState.ResourceDefinition.Id
                                                    ,productionOrderId: featureActivity.ProductionOrderId
                                                          ,operationId:featureActivity.OperationId
                                                    ,activityId:featureActivity.ActivityId
                                                    , activityType: featureActivity.ActivityType
                                                    , duration: randomizedDuration
                                                    , name:featureActivity.Name
                                                    , ganttPlanningInterval:featureActivity.GanttPlanningInterval
                                                    , hub:featureActivity.Hub
                                                    , capability:capability );

                var startActivityInstruction = Resource.Instruction.Central
                                                .ActivityStart.Create(activity: fCentralActivity
                                                                       , target: resourceState.ResourceDefinition.AgentRef);
                if(resourceState.ActivityQueue.Peek().GetKey == featureActivity.Key)
                {
                    resourceState.ActivityQueue.Dequeue();
                    _activityManager.AddOrUpdateActivity(activity, resourceState.ResourceDefinition);
                    resourceState.StartActivityAtResource(activity);
                    Agent.Send(startActivityInstruction);
                }
                else
                {
                    Agent.DebugMessage($"{resourceState.ResourceDefinition.Name} has another activity");
                }

            }

        }
        
        public void FinishActivity(FCentralActivity fActivity)
        {
            // Get Resource
            System.Diagnostics.Debug.WriteLine($"Finish {fActivity.ProductionOrderId}|{fActivity.OperationId}|{fActivity.ActivityId} with Duration: {1} from {2}", fActivity.Name , fActivity.Duration, Agent.Sender.ToString());

            var activity = _resourceManager.GetCurrentActivity(fActivity.ResourceId);
           
            // Add Confirmation
            _activityManager.FinishActivityForResource(activity, fActivity.ResourceId);

            if (_activityManager.ActivityIsFinished(activity.GetKey))
            {
                //Check if productionorder finished

                _confirmationManager.AddConfirmations(activity, GanttConfirmationState.Finished);

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
                var prodactivity = _activityManager.Activities.SingleOrDefault(x => x.Activity.Equals(activityOfProductionOrder));

                if (prodactivity == null || !prodactivity.ActivityIsFinishedDebug())
                {
                    return;
                }
            }

            Agent.DebugMessage($"All resources for {activity.GetKey} returned!");

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
                        var materialActivity = _activityManager.GetActivity(material.GetChildKey);
                        materialId = materialActivity.Productionorder.MaterialId;
                        break;
                    case 8:
                        materialId = material.ChildId;
                        break;
                    default:
                            throw new Exception("materialrelationtype does not exits");
                }
                var stockPosting = new FCentralStockPosting(materialId: materialId, (double)material.Quantity);
                Agent.Send(DirectoryAgent.Directory.Instruction.Central.WithdrawMaterial.Create(stockPosting,Agent.ActorPaths.StorageDirectory.Ref));

            }
        }

        public override bool AfterInit()
        {
            Agent.Send(Hub.Instruction.Central.StartActivities.Create(Agent.Context.Self), 1);
            return true;
        }

        #region Reporting

        public void CreateSimulationJob(GptblProductionorderOperationActivity activity, long start, long duration, string requiredCapabilityName)
        {
            var pub = MessageFactory.ToSimulationJob(activity, start, duration, requiredCapabilityName);
            Agent.Context.System.EventStream.Publish(@event: pub);
        }
        #endregion

    }
}
