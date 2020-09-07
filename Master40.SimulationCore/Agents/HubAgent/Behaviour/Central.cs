using Master40.DB.Nominal;
using Master40.SimulationCore.Helper.DistributionProvider;
using System;
using Akka;
using Master40.DB.Data.Context;
using Microsoft.EntityFrameworkCore;
using SQLitePCL;
using static FResourceInformations;
using System.Linq;

namespace Master40.SimulationCore.Agents.HubAgent.Behaviour
{
    class Central : SimulationCore.Types.Behaviour
    {
        private GanttPlanDBContext _ganttPlanDbContext { get; }
        private WorkTimeGenerator _workTimeGenerator { get; }
        //private ContractNetManager _contractNetManager { get; }

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
                    case Hub.Instruction.Central.LoadProducationOrderNet msg: LoadContractNet(); break;
                    //

                    case Hub.Instruction.Default.EnqueueJob msg: EnqueueJob(); break;
                    default: return false;
                }
                return true;
            }

        private void LoadContractNet()
        {
            var prod = _ganttPlanDbContext.GptblProductionorder
                .Include(x => x.ProductionorderOperationActivities)
                .ThenInclude(x => x.ProductionorderOperationActivityMaterialrelation)
                .Include(x => x.ProductionorderOperationActivities)
                .ThenInclude(x => x.ProductionorderOperationActivityResources)
                .Include(x => x.ProductionorderOperationActivities)
                .ThenInclude(x => x.ProductionorderOperationActivityResources)
                .ThenInclude(x => x.ProductionorderOperationActivityResourceInterval)
                .Single(x => x.ProductionorderId == "000010");

            System.Diagnostics.Debug.WriteLine($"ProductionOrder for {prod.MaterialId} at Hub");

            var contractNet = _ganttPlanDbContext.GptblProductionorder
                .Include(x => x.ProductionorderOperationActivities)
                .ThenInclude(x => x.ProductionorderOperationActivityMaterialrelation)
                .Include(x => x.ProductionorderOperationActivities)
                .ThenInclude(x => x.ProductionorderOperationActivityResources)
                .Include(x => x.ProductionorderOperationActivities)
                .ThenInclude(x => x.ProductionorderOperationActivityResources)
                .ThenInclude(x => x.ProductionorderOperationActivityResourceInterval);




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
