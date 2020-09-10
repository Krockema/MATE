using Akka.Actor;
using Master40.DB.Data.Context;
using Master40.DB.Nominal;
using Master40.SimulationCore.Agents.HubAgent.Types;
using Master40.SimulationCore.Agents.HubAgent.Types.Central;
using Master40.SimulationCore.Helper.DistributionProvider;
using Master40.Tools.Connectoren.Ganttplan;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using Master40.DB.Data.Helper;
using Master40.DB.GanttPlanModel;
using static FResourceInformations;
using static FCentralResourceRegistrations;
using static FCentralActivities;

namespace Master40.SimulationCore.Agents.HubAgent.Behaviour
{
    class Central : SimulationCore.Types.Behaviour
    {
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

            var resourceIntervals = _ganttPlanDbContext.GptblProductionorderOperationActivityResourceInterval
                .Include(x => x.ProductionorderOperationActivityResource)
                .ThenInclude(x => x.ProductionorderOperationActivity)
                .ThenInclude(x => x.ProductionorderOperationActivityMaterialrelation)
                .Include(x => x.ProductionorderOperationActivityResource)
                .ThenInclude(x => x.ProductionorderOperationActivity)
                .ThenInclude(x => x.Productionorder)
                .Include(x => x.ProductionorderOperationActivityResource)
                .ThenInclude(x => x.ProductionorderOperationActivity)
                .ThenInclude(x => x.ProductionorderOperationActivityResources).ToList();
            
            _productionOrderManager.Update(resourceIntervals);

            inboxActorRef.Tell("GanttPlan finished!", this.Agent.Context.Self);

        }

        private void AllocateActivities()
        {
            
            foreach (var resource in _resourceManager.resourceWorkList)
            {

                //Skip if resource is working
                if(resource.IsWorking)
                    continue;

                var nextIntervalForResource = _productionOrderManager.GetNextInterval(resource.Id);
                var nextActivityForResource = nextIntervalForResource.ProductionorderOperationActivityResource
                    .ProductionorderOperationActivity;

                //TODO DateTimeConverter to be implemented
                if (!nextIntervalForResource.DateFrom.Equals(Agent.CurrentTime))
                {
                    var featureActivity = new FCentralActivity(resource.Id, nextIntervalForResource.ProductionorderId, nextIntervalForResource.OperationId, nextIntervalForResource.ActivityId, _productionOrderManager._ganttPlanningId);
                    //TODO Change DateTime
                    Agent.Send(instruction: Hub.Instruction.Central.TryStartActivity.Create(featureActivity, Agent.Context.Self), nextIntervalForResource.DateFrom.Minute);
                }

               
                var resourcesForActivity = nextActivityForResource.ProductionorderOperationActivityResources;
                
                //activity can be ignored as long any resource is working -> after finish work of the resource it will trigger anyways
                foreach (var resourceForActivity in resourcesForActivity)
                {
                    if (_resourceManager.ResourceIsWorking(resourceForActivity.ResourceId));
                        return;
                }

                StartActivityAtResources(nextActivityForResource);

            }

        }

        private void TryStartActivity(FCentralActivity activity)
        {
            //Check if planning_id is equal
            if (!activity.GanttPlanningInterval.Equals(_productionOrderManager._ganttPlanningId))
                return;



        }

        
        private void StartActivityAtResources(GptblProductionorderOperationActivity activity)
        {
            var resourcesForActivity = activity.ProductionorderOperationActivityResources;

            foreach (var resourceForActivity in resourcesForActivity)
            {
                var resource =
                    _resourceManager.resourceWorkList.Single(x => x.Id.Equals(resourceForActivity.ResourceId));

            }


        }


        public override bool AfterInit()
        {
            return base.AfterInit();
        }


    }
}
