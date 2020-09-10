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
using static FCentralActivities;
using static FCentralResourceRegistrations;

namespace Master40.SimulationCore.Agents.HubAgent.Behaviour
{
    class Central : SimulationCore.Types.Behaviour
    {
        private Dictionary<int, FCentralActivity> _scheduledActivities { get; } = new Dictionary<int, FCentralActivity>();
        private GanttPlanDBContext _ganttPlanDbContext { get; }
        private WorkTimeGenerator _workTimeGenerator { get; }
        private ProductionOrderManager _productionOrderManager { get; } = new ProductionOrderManager();
        private ResourceManager _resourceManager { get; } = new ResourceManager();

        public Central(string dbConnectionStringGanttPlan, WorkTimeGenerator workTimeGenerator, SimulationType simulationType = SimulationType.Default) : base(childMaker: null, simulationType: simulationType)
        {
            _workTimeGenerator = workTimeGenerator;
            _ganttPlanDbContext = GanttPlanDBContext.GetContext(dbConnectionStringGanttPlan);
        }

            public override bool Action(object message)
            {
                switch (message)
                {
                    case Hub.Instruction.Central.AddResourceToHub msg: AddResourceToHub(msg.GetResourceRegistration); break;
                    case Hub.Instruction.Central.LoadProductionOrders msg: LoadProductionOrders(msg.GetInboxActorRef); break;
                    case Hub.Instruction.Central.TryStartActivity msg: TryStartActivity(msg.GetActivity); break; 
                    case Resource.Instruction.Central.ActivityFinish msg: FinishActivity(msg.GetObjectFromMessage); break;
                    default: return false;
                }
                return true;
            }

        private void AddResourceToHub(FCentralResourceRegistration resourceRegistration)
        {
            _resourceManager.Add(resourceRegistration.ResourceName, resourceRegistration.ResourceId, resourceRegistration.ResourceActorRef);
        }

        private void LoadProductionOrders(IActorRef inboxActorRef)
        {
            System.Diagnostics.Debug.WriteLine("Start GanttPlan");
            GanttPlanOptRunner.RunOptAndExport();
            System.Diagnostics.Debug.WriteLine("Finish GanttPlan");



            foreach (var resource in _resourceManager.resourceWorkList)
            {
                // put to an sorted Queue on each Resource
                resource.ActivityQueue = new Queue<GptblProductionorderOperationActivityResourceInterval>(
                    _ganttPlanDbContext.GptblProductionorderOperationActivityResourceInterval
                        .Include(x => x.ProductionorderOperationActivityResource)
                            .ThenInclude(x => x.ProductionorderOperationActivity)
                                .ThenInclude(x => x.ProductionorderOperationActivityMaterialrelation)
                        .Include(x => x.ProductionorderOperationActivityResource)
                            .ThenInclude(x => x.ProductionorderOperationActivity)
                                .ThenInclude(x => x.Productionorder)
                        .Include(x => x.ProductionorderOperationActivityResource)
                            .ThenInclude(x => x.ProductionorderOperationActivity)
                                .ThenInclude(x => x.ProductionorderOperationActivityResources)
                        .OrderBy(x => x.DateFrom)
                        .ToList());
            } // filter Done and in Progress ? 

            _productionOrderManager.IncrementPlaningNumber();
            _scheduledActivities.Clear();
            inboxActorRef.Tell("GanttPlan finished!", this.Agent.Context.Self);

        }

        private void AllocateActivities()
        {
            foreach (var resource in _resourceManager.resourceWorkList)
            {
                //Skip if resource is working
                if(resource.IsWorking) continue;
                
                var interval = resource.ActivityQueue.Peek();
                var featureActivity = new FCentralActivity(resource.Id
                    , interval.ProductionorderId
                    , interval.OperationId
                    , interval.ConvertedDateTo - interval.ConvertedDateFrom
                    , interval.ProductionorderOperationActivityResource.ProductionorderOperationActivity.Name
                    , interval.ActivityId
                    , _productionOrderManager.PlanVersion
                    , Agent.Context.Self); // may not required.
                
                // Feature Activity
                if (!interval.ConvertedDateFrom.Equals(Agent.CurrentTime))
                {
                    // only schedule activities that have not been scheduled
                    if(_scheduledActivities.TryAdd(interval.ActivityId, featureActivity))
                    {
                        Agent.Send(instruction: Hub.Instruction.Central.TryStartActivity.Create(featureActivity, Agent.Context.Self)
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
        /// </summary>
        /// <param name="featureActivity"></param>
        private void TryStartActivity(FCentralActivity featureActivity)
        {
            if (_scheduledActivities.ContainsKey(featureActivity.ActivityId))
            {
                var resource = _resourceManager.resourceWorkList.Single(x => x.Id == featureActivity.ResourceId);
                if (NextActivityIsEqual(resource, featureActivity.ActivityId))
                {
                    _scheduledActivities.Remove(featureActivity.ActivityId);
                    TryStartActivity(resource.ActivityQueue.Dequeue(), featureActivity);
                }
            }
        }

        private void TryStartActivity(GptblProductionorderOperationActivityResourceInterval interval,FCentralActivity activity)
        {
            // not shure if this works or if we just loop through resources and check even activities
            var resourcesForActivity = interval.ProductionorderOperationActivityResource
                                                .ProductionorderOperationActivity
                                                .ProductionorderOperationActivityResources;

            var requiredResources = new List<ResourceState>();
            //activity can be ignored as long any resource is working -> after finish work of the resource it will trigger anyways
            foreach (var resourceForActivity in resourcesForActivity)
            {
                var resource = _resourceManager.resourceWorkList.Single(x => x.Id == resourceForActivity.ResourceId);
                if (_resourceManager.ResourceIsWorking(resourceForActivity.ResourceId)
                    || !NextActivityIsEqual(resource, interval.ActivityId))
                    return;
                requiredResources.Add(resource);
            }

            StartActivity(requiredResources: requiredResources
                                   ,interval:  interval
                                   ,featureActivity: activity);
        }
        private bool NextActivityIsEqual(ResourceState resource, int activityId)
        {
            var activity = resource.ActivityQueue.Peek();
            return activity != null && activity.ActivityId == activityId;
        }

        
        private void StartActivity(List<ResourceState> requiredResources
            , GptblProductionorderOperationActivityResourceInterval interval
            , FCentralActivity featureActivity)
        {
            var activity = interval.ProductionorderOperationActivityResource.ProductionorderOperationActivity;

            foreach (var resourceForActivity in requiredResources)
            {
                var startActivityInstruction = Resource.Instruction.Central
                                                .ActivityStart.Create(activity: featureActivity
                                                                       ,target: resourceForActivity.AgentRef);
                resourceForActivity.StartActivityAtResource(activity);
                Agent.Send(startActivityInstruction);
            }
        }

        public void FinishActivity(FCentralActivity activity)
        {
            // Get Resource
            System.Diagnostics.Debug.WriteLine("Finish Activity {0} with Duration: {1}", activity.Name ,activity.Duration);
            // Shedule next Activity if Required
            // Check Material
        }


        public override bool AfterInit()
        {
            AllocateActivities();
            return true;
        }


    }
}
