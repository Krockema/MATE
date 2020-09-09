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
using static FResourceInformations;

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
                    //Initialize
                    case Hub.Instruction.Default.AddResourceToHub msg: CreateOperations(resourceInformation: msg.GetObjectFromMessage); break;
                    //After OptRun
                    case Hub.Instruction.Central.LoadProductionOrders msg: LoadProductionOrders(msg.GetInboxActorRef); break;
                //FinishWork
                    //case Hub.Instruction.Central.ResponseFromResource msg: ResponseFromResource() break;
                    case Hub.Instruction.Default.EnqueueJob msg: EnqueueJob(); break;
                    default: return false;
                }
                return true;
            }

        private void LoadProductionOrders(IActorRef inboxActorRef)
        {
            System.Diagnostics.Debug.WriteLine("Start GanttPlan");
            GanttPlanOptRunner.RunOptAndExport();
            System.Diagnostics.Debug.WriteLine("Finish GanttPlan");

            var productionorders = _ganttPlanDbContext.GptblProductionorder;
            
            foreach (var productionorder in productionorders)
            {
                var prod = _ganttPlanDbContext.GptblProductionorder
                    .Include(x => x.ProductionorderOperationActivities)
                    .ThenInclude(x => x.ProductionorderOperationActivityMaterialrelation)
                    .Include(x => x.ProductionorderOperationActivities)
                    .ThenInclude(x => x.ProductionorderOperationActivityResources)
                    .Include(x => x.ProductionorderOperationActivities)
                    .ThenInclude(x => x.ProductionorderOperationActivityResources)
                    .ThenInclude(x => x.ProductionorderOperationActivityResourceInterval)
                    .Single(x => x.ProductionorderId == productionorder.ProductionorderId);

                System.Diagnostics.Debug.WriteLine($"ProductionOrder for {prod.MaterialId} at Hub");


                _productionOrderManager.UpdateOrCreate(prod);

            }

            inboxActorRef.Tell("GanttPlan finished!", this.Agent.Context.Self);

        }


        public override bool AfterInit()
        {
            return base.AfterInit();
        }


        private void EnqueueJob()
        {
            throw new NotImplementedException();
        }

        private void CreateOperations(FResourceInformation resourceInformation)
        {
            throw new NotImplementedException();
        }
    }
}
