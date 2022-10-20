using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Akka.Actor;
using Akka.Util.Internal;
using Mate.DataCore.Data.Context;
using Mate.DataCore.GanttPlan;
using Mate.DataCore.GanttPlan.GanttPlanModel;
using Mate.DataCore.Nominal;
using Mate.DataCore.Nominal.Model;
using Mate.Production.Core.Agents.HubAgent.Types.Central;
using Mate.Production.Core.Helper;
using Mate.Production.Core.Helper.DistributionProvider;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using static FCentralActivities;
using Resource = Mate.Production.Core.Agents.ResourceAgent.Resource;

namespace Mate.Production.Core.Agents.HubAgent.Behaviour
{
    class Central : Core.Types.Behaviour
    {
        private Dictionary<string, FCentralActivity> _scheduledActivities { get; } = new Dictionary<string, FCentralActivity>();
        private string _dbConnectionStringGanttPlan { get; }
        private string _dbConnectionStringMaster { get; }
        private WorkTimeGenerator _workTimeGenerator { get; }
        private PlanManager _planManager { get; } = new PlanManager();
        private ResourceManager _resourceManager { get; } = new ResourceManager();
        private ActivityManager _activityManager { get; } = new ActivityManager();
        private ConfirmationManager _confirmationManager { get; }
        private StockPostingManager _stockPostingManager { get; } 
        private List<GptblSalesorder> _salesOrder { get; set; } = new List<GptblSalesorder>();
        private List<GptblMaterial> _materials { get; } = new List<GptblMaterial>();
        private List<GptblSalesorderMaterialrelation> _SalesorderMaterialrelations { get; set;  } = new List<GptblSalesorderMaterialrelation>();
        private Dictionary<string, string> _prtResources { get; set; } = new Dictionary<string, string>();

        public Central(string dbConnectionStringGanttPlan, string dbConnectionStringMaster, WorkTimeGenerator workTimeGenerator, SimulationType simulationType = SimulationType.Default) : base(childMaker: null, simulationType: simulationType)
        {
            _workTimeGenerator = workTimeGenerator;
            _dbConnectionStringGanttPlan = dbConnectionStringGanttPlan;
            _dbConnectionStringMaster = dbConnectionStringMaster;
            _confirmationManager = new ConfirmationManager(dbConnectionStringGanttPlan);
            _stockPostingManager = new StockPostingManager(dbConnectionStringGanttPlan);

            using (var localganttplanDB = GanttPlanDBContext.GetContext(_dbConnectionStringGanttPlan))
            {
                _materials = localganttplanDB.GptblMaterial.ToList();
            }

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

        private void AddResourceToHub(FCentralResourceRegistrations.FCentralResourceRegistration resourceRegistration)
        {
            var resourceDefintion = new ResourceDefinition(resourceRegistration.ResourceName, resourceRegistration.ResourceId, resourceRegistration.ResourceActorRef, resourceRegistration.ResourceGroupId, (ResourceType)resourceRegistration.ResourceType);
            _resourceManager.Add(resourceDefintion);
        }

        private void LoadProductionOrders(IActorRef inboxActorRef)
        {
            var agentDateTime = Agent.CurrentTime.ToDateTime();
            var timeStamps = new Dictionary<string, long>();
            long lastStep = 0;
            var stopwatch = Stopwatch.StartNew();
            stopwatch.Start();

            //_stockPostingManager.TransferStockPostings();
            _confirmationManager.TransferConfirmations();

            /*using (var localGanttPlanDbContext = GanttPlanDBContext.GetContext(_dbConnectionStringGanttPlan))
            {
                foreach (var updatedSalesorder in _salesOrder.Where(x => x.Status.Equals(8)))
                {
                    var salesorder = localGanttPlanDbContext.GptblSalesorder.Single(x => x.SalesorderId.Equals(updatedSalesorder.SalesorderId));

                    if(salesorder.Status.NotEqual(8)){
                        salesorder.Status = updatedSalesorder.Status;
                        salesorder.QuantityDelivered = updatedSalesorder.QuantityDelivered;
                    }

                }

                localGanttPlanDbContext.SaveChanges();
            }*/

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

            lastStep = stopwatch.ElapsedMilliseconds;
            timeStamps.Add("Write Confirmations", lastStep);
            CreateComputationalTime("TimeWriteConfirmations", lastStep);

            System.Diagnostics.Debug.WriteLine("Start GanttPlan");
            GanttPlanOptRunner.RunOptAndExport("Continuous", "D:\\Work\\GANTTPLAN\\GanttPlanOptRunner.exe"); //changed to Init - merged configs
            System.Diagnostics.Debug.WriteLine("Finish GanttPlan");

            lastStep = stopwatch.ElapsedMilliseconds - lastStep;
            timeStamps.Add("Gantt Plan Execution", lastStep);
            CreateComputationalTime("TimeGanttPlanExecution", lastStep);

            using (var localGanttPlanDbContext = GanttPlanDBContext.GetContext(_dbConnectionStringGanttPlan))
            {
                foreach (var resourceState in _resourceManager.resourceStateList)
                {
                    JobToStabilityManager(resourceState.ActivityQueue, resourceState, Mate.Production.Core.Agents.CollectorAgent.Types.Process.Dequeue);

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
                            .Where(x => x.ResourceId.Equals(resourceState.ResourceDefinition.Id.ToString())
                                        && x.IntervalAllocationType.Equals(1)
                                        && (x.ProductionorderOperationActivityResource.ProductionorderOperationActivity.Status != (int)GanttActivityState.Finished
                                        && x.ProductionorderOperationActivityResource.ProductionorderOperationActivity.Status != (int)GanttActivityState.Started))
                            .OrderBy(x => x.DateFrom)
                            .ToList());

                    JobToStabilityManager(resourceState.ActivityQueue, resourceState, Mate.Production.Core.Agents.CollectorAgent.Types.Process.Enqueue);
                } // filter Done and in Progress?

                var tempsalesorder = _SalesorderMaterialrelations;
                _SalesorderMaterialrelations = localGanttPlanDbContext.GptblSalesorderMaterialrelation.ToList();
                foreach (var salesorder in tempsalesorder)
                {
                    var newProductionOrderId = _SalesorderMaterialrelations.Single(x => x.SalesorderId.Equals(salesorder.SalesorderId)).ChildId;
                    if (newProductionOrderId != salesorder.ChildId)
                    {
                        System.Diagnostics.Debug.WriteLine($"Productionorder for SalesOrderId {salesorder.SalesorderId} changed from {salesorder.ChildId} to {newProductionOrderId}");
                    }
                }
                
                _salesOrder = localGanttPlanDbContext.GptblSalesorder.ToList();

                if(_prtResources.Count <1){
                    localGanttPlanDbContext.GptblPrt.Where(x => x.CapacityType.Equals(1)).Select(x => new { x.Id, x.Name}).ForEach(x => _prtResources.Add(x.Id, x.Name));
                }
            }

            //delete orders to avoid sync
            using (var masterDBContext = MateDb.GetContext(_dbConnectionStringMaster))
            {
                masterDBContext.CustomerOrders.RemoveRange(masterDBContext.CustomerOrders);
                masterDBContext.CustomerOrderParts.RemoveRange(masterDBContext.CustomerOrderParts);
                masterDBContext.SaveChanges();
            }

            Agent.DebugMessage($"GanttPlanning number {_planManager.PlanVersion} incremented {_planManager.IncrementPlaningNumber()}");
            _scheduledActivities.Clear();

            StartActivities();

            lastStep = stopwatch.ElapsedMilliseconds - lastStep;
            timeStamps.Add("Load Gantt Results", lastStep);
            timeStamps.Add("TimeStep", Agent.CurrentTime);
            stopwatch.Stop();
            CreateComputationalTime("TimeTotalExecution", lastStep);

            inboxActorRef.Tell(new FCentralGanttPlanInformations.FCentralGanttPlanInformation(JsonConvert.SerializeObject(timeStamps), "Plan"), this.Agent.Context.Self);
        }

        private void JobToStabilityManager(Queue<GptblProductionorderOperationActivityResourceInterval> queue, ResourceState resource, Mate.Production.Core.Agents.CollectorAgent.Types.Process process)
        {
            if (queue == null || queue.Count == 0 )
                return;

            for (int i = 0; i < queue.Count; i++)
            {
                AddToStabilityManager(queue.ElementAt(i), i, resource, process);
            }
        }
        private void AddToStabilityManager(GptblProductionorderOperationActivityResourceInterval job, int position, ResourceState resource, Mate.Production.Core.Agents.CollectorAgent.Types.Process process)
        {
            if (resource.ResourceDefinition.ResourceType != ResourceType.Workcenter)
                return;

            var operationKeys = new List<string>(){ job.GetKey};
            var pub = new FCreateStabilityMeasurements.FCreateStabilityMeasurement(
                keys: operationKeys
                , time: Agent.CurrentTime
                , position: position
                , resource: resource.ResourceDefinition.Name
                , start: job.DateFrom.ToSimulationTime()
                , process: process.ToString()
                );

            Agent.Context.System.EventStream.Publish(@event: pub);

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
                
                var fActivity = new FCentralActivity(resourceState.ResourceDefinition.Id
                    , interval.ProductionorderId
                    , interval.OperationId
                    , interval.ActivityId
                    , Agent.CurrentTime
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
                    if(_scheduledActivities.TryAdd(fActivity.Key, fActivity))
                    {   
                        var waitFor = interval.ConvertedDateFrom - Agent.CurrentTime;
                        Agent.DebugMessage($"{interval.ProductionorderOperationActivityResource.ProductionorderOperationActivity.GetKey} has been scheduled to {Agent.CurrentTime + waitFor} as planning interval {_planManager.PlanVersion}");
                        Agent.Send(instruction: Hub.Instruction.Central.ScheduleActivity.Create(fActivity, Agent.Context.Self)
                                            , waitFor);
                    }
                }
                else
                {
                    if (interval.ConvertedDateFrom < Agent.CurrentTime)
                    {
                        Agent.DebugMessage($"Activity {interval.ProductionorderOperationActivityResource.ProductionorderOperationActivity.GetKey} at {resourceState.ResourceDefinition.Name} is delayed {interval.ConvertedDateFrom}");
                    }
                    
                    // Activity is scheduled for now
                    TryStartActivity(interval, fActivity);
                }
            }
        }

        /// <summary>
        /// Check if Feature is still active.
        /// 
        /// </summary>
        /// <param name="fActivity"></param>
        private void ScheduleActivity(FCentralActivity fActivity)
        {
            if (fActivity.GanttPlanningInterval < _planManager.PlanVersion)
            {
                Agent.DebugMessage($"{fActivity.Key} has new schedule from more recent planning interval {fActivity.GanttPlanningInterval}");
                return;
            }

            if (_activityManager.Activities.Any(x => x.Activity.GetKey.Equals(fActivity.Key)))
            {
                Agent.DebugMessage($"Actvity {fActivity.Key} already in progress");
                return;
            }

            //Agent.DebugMessage($"TryStart scheduled activity {fActivity.Key}");
            if (_scheduledActivities.ContainsKey(fActivity.Key))
            {
                var resource = _resourceManager.resourceStateList.Single(x => x.ResourceDefinition.Id == fActivity.ResourceId);
                if (NextActivityIsEqual(resource, fActivity.Key))
                {
                    Agent.DebugMessage($"Activity {fActivity.Key} is equal start now after been scheduled by {fActivity.ResourceId}");
                    _scheduledActivities.Remove(fActivity.Key);
                    TryStartActivity(resource.ActivityQueue.Peek(), fActivity);
                }
            }
        }

        private void TryStartActivity(GptblProductionorderOperationActivityResourceInterval interval,FCentralActivity fActivity)
        {
            var resourcesForActivity = interval.ProductionorderOperationActivityResource
                                                .ProductionorderOperationActivity
                                                .ProductionorderOperationActivityResources;

            var resourceId = resourcesForActivity.SingleOrDefault(x => x.ResourceType.Equals(5))?.ResourceId;
            var requiredCapability = _prtResources[resourceId];
            var resource = _resourceManager.resourceStateList.Single(x => x.ResourceDefinition.Id.Equals(fActivity.ResourceId));

            Agent.DebugMessage($"{resource.ResourceDefinition.Name} try start activity {fActivity.Key}!");

            var requiredResources = new List<ResourceState>();
            var resourceCount = 0;

            //activity can be ignored as long any resource is working -> after finish work of the resource it will trigger anyways
            foreach (var resourceForActivity in resourcesForActivity)
            {
                if (_prtResources.ContainsKey(resourceForActivity.ResourceId))
                {
                    //do not request, because tool is infinity
                    resourceCount++;
                    Agent.DebugMessage($"{resourceCount} of {resourcesForActivity.Count} Resource for Activity {fActivity.Key} are ready");
                    continue;
                }
                var resourceState = _resourceManager.resourceStateList.Single(x => x.ResourceDefinition.Id == int.Parse(resourceForActivity.ResourceId));

                if(!_resourceManager.ResourceReadyToWorkOn(int.Parse(resourceForActivity.ResourceId), fActivity.Key))
                {
                    Agent.DebugMessage($"{resourceState.ResourceDefinition.Name} is working on {resourceState.GetCurrentProductionOperationActivity}. {fActivity.Key} stopped!");

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
                                , fActivity: fActivity
                                , requiredCapability: requiredCapability);
        }

        private bool NextActivityIsEqual(ResourceState resourceState, string activityKey)
        {
            var nextActivity = resourceState.ActivityQueue.Peek();
            if (nextActivity == null)
            {
                Agent.DebugMessage($"No next activity for {resourceState.ResourceDefinition.Name} in resource activity queue");
                return false;
            }
            return nextActivity.GetKey == activityKey;
        }

        
        private void StartActivity(List<ResourceState> requiredResources
            , GptblProductionorderOperationActivityResourceInterval interval
            , FCentralActivity fActivity
            , string requiredCapability)
        {
            if (_scheduledActivities.ContainsKey(fActivity.Key))
            {
                Agent.DebugMessage($"Activity {fActivity.Key} removed from _scheduledActivities");
                _scheduledActivities.Remove(fActivity.Key);
            }

            var activity = interval.ProductionorderOperationActivityResource.ProductionorderOperationActivity;

            Agent.DebugMessage($"Start activity {fActivity.ProductionOrderId}|{fActivity.OperationId}|{fActivity.ActivityId}!");

            WithdrawalMaterial(activity);

            _confirmationManager.AddConfirmations(activity: activity,
                                                  confirmationType: GanttConfirmationState.Started,
                                                  currentTime: Agent.CurrentTime,
                                                  activityStart: Agent.CurrentTime);

            var randomizedDuration = fActivity.Duration;

            //only randomize ActivityType "Operation"
            if (fActivity.ActivityId == 3)
            {
                
                randomizedDuration = _workTimeGenerator.GetRandomWorkTime(fActivity.Duration, fActivity.Key.GetHashCode());
            }

            CreateSimulationJob(activity, Agent.CurrentTime, randomizedDuration, requiredCapability);

            foreach (var resourceState in requiredResources)
            {

                var fCentralActivity = new FCentralActivity(resourceId: resourceState.ResourceDefinition.Id
                                                    , productionOrderId: fActivity.ProductionOrderId
                                                    , operationId: fActivity.OperationId
                                                    , activityId: fActivity.ActivityId
                                                    , activityType: fActivity.ActivityType
                                                    , start: Agent.CurrentTime
                                                    , duration: randomizedDuration
                                                    , name: fActivity.Name
                                                    , ganttPlanningInterval:fActivity.GanttPlanningInterval
                                                    , hub: fActivity.Hub
                                                    , capability: requiredCapability);

                var startActivityInstruction = Resource.Instruction.Central
                                                .ActivityStart.Create(activity: fCentralActivity
                                                                       , target: resourceState.ResourceDefinition.AgentRef);

                if (resourceState.ActivityQueue.Peek().GetKey == fActivity.Key)
                {
                    Agent.DebugMessage($"{activity.GetKey} has been dequeued {resourceState.ResourceDefinition.Name}");
                    resourceState.ActivityQueue.Dequeue();
                    _activityManager.AddOrUpdateActivity(activity: activity,
                                                         resourceDefinition: resourceState.ResourceDefinition,
                                                         planVersion: _planManager.PlanVersion);
                    resourceState.StartActivityAtResource(activity);
                    Agent.Send(startActivityInstruction);
                }
                else
                {
                    Agent.DebugMessage($"{activity.GetKey} cant start because {resourceState.ResourceDefinition.Name} has {resourceState.ActivityQueue.Peek().GetKey} next");
                }


            }

        }
        
        public void FinishActivity(FCentralActivity fActivity)
        {
            // Get Resource
            System.Diagnostics.Debug.WriteLine($"Finish {fActivity.ProductionOrderId}|{fActivity.OperationId}|{fActivity.ActivityId} with Duration: {fActivity.Duration} from {Agent.Sender.ToString()}");

            var activity = _resourceManager.GetCurrentActivity(fActivity.ResourceId);

            // Add Confirmation
            _activityManager.FinishActivityForResource(activity, fActivity.ResourceId);

            if (_activityManager.ActivityIsFinished(activity.GetKey))
            {

                //Check if productionorder finished
                _confirmationManager.AddConfirmations(activity: activity,
                                                      confirmationType: GanttConfirmationState.Finished,
                                                      currentTime: Agent.CurrentTime,
                                                      activityStart: fActivity.Start);

                Agent.DebugMessage($"All acvitivities for {activity.GetKey} are finished yet. Try finish ProductionOrder");

                ProductionOrderFinishCheck(activity);
            }

            // Finish ResourceState
            _resourceManager.FinishActivityAtResource(fActivity.ResourceId);

            //Check If new activities are available
            StartActivities();
        }

        private void ProductionOrderFinishCheck(GptblProductionorderOperationActivity activity)
        {
            var productionOrder = activity.Productionorder;

            foreach (var activityOfProductionOrder in productionOrder.ProductionorderOperationActivities)
            {
                var prodactivity = _activityManager.Activities.SingleOrDefault(x => x.Activity.Equals(activityOfProductionOrder));

                if (prodactivity == null || !prodactivity.ActivityIsFinished())
                {
                    Agent.DebugMessage($"Not all acvitivities for { productionOrder.ProductionorderId} are finished yet");
                    return;
                }
            }

            Agent.DebugMessage($"{activity.GetKey} was last activity of productionorder {productionOrder.ProductionorderId}. Finished MaterialId {productionOrder.MaterialId}!");

            // Insert Material
            _stockPostingManager.AddInsertStockPosting(
                   materialId: productionOrder.MaterialId,
                   quantity: productionOrder.QuantityNet.Value,
                   materialQuantityUnitId: productionOrder.QuantityUnitId,
                   stockPostingType: GanttStockPostingType.Relatively,
                   CurrentTime: Agent.CurrentTime);

            var storagePosting = new FCentralStockPostings.FCentralStockPosting(productionOrder.MaterialId, (double)productionOrder.QuantityNet);
            Agent.Send(DirectoryAgent.Directory.Instruction.Central.ForwardInsertMaterial.Create(storagePosting, Agent.ActorPaths.StorageDirectory.Ref));

            //only for products
            var product = _materials.Where(x => x.Info1.Equals("Product")).SingleOrDefault(x => x.MaterialId.Equals(productionOrder.MaterialId));
            if (product != null)
            {
                var salesorderId = _SalesorderMaterialrelations.Single(x => x.ChildId.Equals(productionOrder.ProductionorderId)).SalesorderId;

                var salesorder = _salesOrder.Single(x => x.SalesorderId.Equals(salesorderId));

                CreateLeadTime(product, long.Parse(salesorder.Info1));
                
                salesorder.Status = 8;
                salesorder.QuantityDelivered = 1;

                var provideOrder = new FCentralProvideOrders.FCentralProvideOrder(productionOrderId: productionOrder.ProductionorderId
                                                                 ,materialId : product.MaterialId
                                                                ,materialName: product.Name
                                                               , salesOrderId: salesorderId
                                                         , materialFinishedAt: Agent.CurrentTime);

                 Agent.Send(DirectoryAgent.Directory.Instruction.Central.ForwardProvideOrder.Create(provideOrder, Agent.ActorPaths.StorageDirectory.Ref));
                

            }

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

                _stockPostingManager.AddWithdrawalStockPosting(materialId, (double)material.Quantity, material.QuantityUnitId, GanttStockPostingType.Relatively, Agent.CurrentTime);

                var stockPosting = new FCentralStockPostings.FCentralStockPosting(materialId: materialId, (double)material.Quantity);
                Agent.Send(DirectoryAgent.Directory.Instruction.Central.ForwardWithdrawMaterial.Create(stockPosting,Agent.ActorPaths.StorageDirectory.Ref));

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
            var pub = activity.ToSimulationJob(start, duration, requiredCapabilityName);
            Agent.Context.System.EventStream.Publish(@event: pub);
        }

        private void CreateLeadTime(GptblMaterial material, long creationTime)
        {
                var pub = new FThroughPutTimes.FThroughPutTime(articleKey: Guid.Empty
                    , articleName: material.Name
                    , start: creationTime  // TODO  ADD CreationTime
                    , end: Agent.CurrentTime);
                Agent.Context.System.EventStream.Publish(@event: pub);
        }
        private void CreateComputationalTime(string type ,long duration)
        {
            var pub = new FComputationalTimers.FComputationalTimer(
                time: Agent.CurrentTime,
                timertype: type,
                duration: duration);
            Agent.Context.System.EventStream.Publish(@event: pub);
        }
        #endregion

    }
}
