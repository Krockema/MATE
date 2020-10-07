using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Akka.TestKit.Xunit;
using Akka.Util.Internal;
using AkkaSim.Logging;
using Master40.DB.Data.Context;
using Master40.DB.Data.Initializer;
using Master40.DB.GanttPlanModel;
using Master40.DB.Nominal;
using Master40.DB.Nominal.Model;
using Master40.Simulation.CLI;
using Master40.SimulationCore;
using Master40.SimulationCore.Environment.Options;
using Master40.SimulationCore.Helper;
using Master40.Tools.Connectoren.Ganttplan;
using Microsoft.EntityFrameworkCore;
using Xunit;
using NLog;

namespace Master40.XUnitTest.SimulationEnvironment
{
    public class CentralSystem : TestKit
    {

        // local Context
        private const string masterCtxString = "Server=(localdb)\\mssqllocaldb;Database=Master40;Trusted_Connection=True;MultipleActiveResultSets=true";
        private const string masterResultCtxString = "Server=(localdb)\\mssqllocaldb;Database=Master40Results;Trusted_Connection=True;MultipleActiveResultSets=true";

        // local TEST Context
        private const string testCtxString = "Server=(localdb)\\mssqllocaldb;Database=TestContext;Trusted_Connection=True;MultipleActiveResultSets=true";
        private const string testResultCtxString = "Server=(localdb)\\mssqllocaldb;Database=TestResultContext;Trusted_Connection=True;MultipleActiveResultSets=true";

        // remote Context
        private const string remoteMasterCtxString = "Server=141.56.137.25,1433;Persist Security Info=False;User ID=SA;Password=123*Start#;Initial Catalog=Master40;MultipleActiveResultSets=true";
        private const string remoteResultCtxString = "Server=141.56.137.25,1433;Persist Security Info=False;User ID=SA;Password=123*Start#;Initial Catalog=Master40Result;MultipleActiveResultSets=true";

        private const string hangfireCtxString = "Server=141.56.137.25;Database=Hangfire;Persist Security Info=False;User ID=SA;Password=123*Start#;MultipleActiveResultSets=true";

        // only for local usage
        private const string GanttPlanCtxString = "SERVER=(localdb)\\MSSQLLocalDB;DATABASE=GanttPlanImportTestDB;Trusted_connection=Yes;UID=;PWD=;MultipleActiveResultSets=true";

        private ProductionDomainContext _masterDBContext = ProductionDomainContext.GetContext(remoteMasterCtxString);
        private ResultContext _ctxResult = ResultContext.GetContext(resultCon: testResultCtxString);

        [Fact]
        public void TestDateUpdate()
        {
            GanttPlanOptRunner.RunOptAndExport("Init");

            var ganttPlanContext = GanttPlanDBContext.GetContext(GanttPlanCtxString);

            var modelparameter = ganttPlanContext.GptblModelparameter.FirstOrDefault();
            if (modelparameter != null)
            {
                modelparameter.ActualTime = new DateTime(year: 2020, day: 2, month: 1);
                ganttPlanContext.Entry(modelparameter).State = Microsoft.EntityFrameworkCore.EntityState.Modified;
                ganttPlanContext.SaveChanges();
            }


            GanttPlanOptRunner.RunOptAndExport("Continuous");

            var modelparameter1 = ganttPlanContext.GptblModelparameter.FirstOrDefault();
            if (modelparameter != null)
            {
                modelparameter1.ActualTime = new DateTime(year: 2020, day: 3, month: 1);
                ganttPlanContext.Entry(modelparameter1).State = Microsoft.EntityFrameworkCore.EntityState.Modified;
                ganttPlanContext.SaveChanges();
            }

            GanttPlanOptRunner.RunOptAndExport("Continuous");

            var modelparameter2 = ganttPlanContext.GptblModelparameter.FirstOrDefault();
            if (modelparameter != null)
            {
                modelparameter2.ActualTime = new DateTime(year: 2020, day: 3, month: 1);
                ganttPlanContext.Entry(modelparameter2).State = Microsoft.EntityFrameworkCore.EntityState.Modified;
                ganttPlanContext.SaveChanges();
            }


        }

        [Fact]
        //[InlineData(SimulationType.DefaultSetup, 1, Int32.MaxValue, 1920, 169, ModelSize.Small, ModelSize.Small)]
        public async Task CentralSystemTest()
        {
            //LogConfiguration.LogTo(TargetTypes.Debugger, TargetNames.LOG_AGENTS, LogLevel.Trace, LogLevel.Trace);
            LogConfiguration.LogTo(TargetTypes.Debugger, TargetNames.LOG_AGENTS, LogLevel.Info, LogLevel.Info);
            //LogConfiguration.LogTo(TargetTypes.Debugger, TargetNames.LOG_AGENTS, LogLevel.Debug, LogLevel.Debug);
            LogConfiguration.LogTo(TargetTypes.Debugger, TargetNames.LOG_AGENTS, LogLevel.Warn, LogLevel.Warn);

            var simtulationType = SimulationType.Central;
            var seed = 169;
            var throughput = 1920;
            var arrivalRate = 0.015;

            //Create Master40Data
            var masterPlanContext = ProductionDomainContext.GetContext(masterCtxString);
            masterPlanContext.Database.EnsureDeleted();
            masterPlanContext.Database.EnsureCreated();
            MasterDBInitializerTruck.DbInitialize(masterPlanContext, ModelSize.Medium, ModelSize.Medium, ModelSize.Small, 3,false);

            //CreateMaster40Result
            var masterPlanResultContext = ResultContext.GetContext(masterResultCtxString);
            masterPlanResultContext.Database.EnsureDeleted();
            masterPlanResultContext.Database.EnsureCreated();
            ResultDBInitializerBasic.DbInitialize(masterPlanResultContext);
            
            //Reset GanttPLan DB?
            var ganttPlanContext = GanttPlanDBContext.GetContext(GanttPlanCtxString);
            ganttPlanContext.Database.ExecuteSqlRaw("EXEC sp_MSforeachtable 'DELETE FROM ? '");
            
            //Synchronisation GanttPlan
            GanttPlanOptRunner.RunOptAndExport("Init");

            var simContext = new GanttSimulation(ganttPlanContext, masterCtxString, messageHub: new ConsoleHub());
            var simConfig = ArgumentConverter.ConfigurationConverter(masterPlanResultContext, 1);
            // update customized Items
            simConfig.AddOption(new DBConnectionString(masterResultCtxString));
            simConfig.ReplaceOption(new KpiTimeSpan(480));
            simConfig.ReplaceOption(new TimeConstraintQueueLength(480));
            simConfig.ReplaceOption(new SimulationKind(value: simtulationType));
            simConfig.ReplaceOption(new OrderArrivalRate(value: arrivalRate));
            simConfig.ReplaceOption(new OrderQuantity(value: 1500));
            simConfig.ReplaceOption(new EstimatedThroughPut(value: throughput));
            simConfig.ReplaceOption(new TimePeriodForThroughputCalculation(value: 1920));
            simConfig.ReplaceOption(new Seed(value: seed));
            simConfig.ReplaceOption(new SettlingStart(value: 2880));
            simConfig.ReplaceOption(new SimulationEnd(value: 10080));
            simConfig.ReplaceOption(new SaveToDB(value: true));
            simConfig.ReplaceOption(new DebugSystem(value: false));
            simConfig.ReplaceOption(new WorkTimeDeviation(0.2));

            var simulation = await simContext.InitializeSimulation(configuration: simConfig);

            //emtpyResultDBbySimulationNumber(simNr: simConfig.GetOption<SimulationNumber>());

            var simWasReady = false;
            if (simulation.IsReady())
            {
                // set for Assert 
                simWasReady = true;
                // Start simulation
                var sim = simulation.RunAsync();
                simContext.StateManager.ContinueExecution(simulation);
                await sim;
            }

            Assert.True(condition: simWasReady);
        }

        [Fact(Skip = "Offline")]
        public void AggreteResults()
        {
            var _resultContext = ResultContext.GetContext(remoteResultCtxString);

            var aggregator = new ResultAggregator(_resultContext);
            aggregator.BuildResults(1);
        }


        [Fact]
        public void TestGanttPlanApi()
        {
            ProductionDomainContext master40Context = ProductionDomainContext.GetContext(masterCtxString);

            GanttPlanDBContext ganttPlanContext = GanttPlanDBContext.GetContext(GanttPlanCtxString);
             var prod = ganttPlanContext.GptblProductionorder
                 .Include(x => x.ProductionorderOperationActivities)
                     .ThenInclude(x => x.ProductionorderOperationActivityMaterialrelation)
                 .Include(x => x.ProductionorderOperationActivities)
                     .ThenInclude(x => x.ProductionorderOperationActivityResources)
                 .Include(x => x.ProductionorderOperationActivities)
                     .ThenInclude(x => x.ProductionorderOperationActivityResources)
                         .ThenInclude(x => x.ProductionorderOperationActivityResourceInterval)
                 .Single(x => x.ProductionorderId == "000030");

            // System.Diagnostics.Debug.WriteLine("First ID: " + prod.ProductionorderId);
            // var activity = prod.ProductionorderOperationActivities.ToArray()[1];
            // System.Diagnostics.Debug.WriteLine("First Activity ID: " + activity.ActivityId);
            // var materialRelation = activity.ProductionorderOperationActivityMaterialrelation.ToArray()[0];
            // System.Diagnostics.Debug.WriteLine("First Activity Material Relation ID: " + materialRelation.ChildId);
            // var ress = activity.ProductionorderOperationActivityResources.ToArray()[0];
            // System.Diagnostics.Debug.WriteLine("First Resource: " + ress.Worker.Name);
            // System.Diagnostics.Debug.WriteLine("First Resource Intervall: " + ress.ProductionorderOperationActivityResourceInterval.DateFrom);
            var activities = ganttPlanContext.GptblProductionorderOperationActivity
                            .Include(x => x.ProductionorderOperationActivityResources)
                                .Where(x => x.ProductionorderId == "000029").ToList();
            activities.ForEach(act =>
            {
                System.Diagnostics.Debug.WriteLine("Activity:" + act.Name);
                act.ProductionorderOperationActivityResources.ForEach(res =>
                {
                    System.Diagnostics.Debug.WriteLine("Activity Resource:" + res.ResourceId);
                    switch (res.ResourceType)
                    {
                        case 1:
                            res.Resource =
                                ganttPlanContext.GptblWorkcenter.Single(x => x.WorkcenterId == res.ResourceId);
                            break;
                        case 3:
                            res.Resource =
                                ganttPlanContext.GptblWorker.Single(x => x.WorkerId == res.ResourceId);
                            break;
                        case 5:
                            res.Resource =
                                ganttPlanContext.GptblPrt.Single(x => x.PrtId == res.ResourceId);

                            break;
                    }
                    System.Diagnostics.Debug.WriteLine("Activity Resource Name:" + res.Resource.Name);
                });
            }
            );


            Assert.True(ganttPlanContext.GptblMaterial.Any());
        }


        [Fact]
        public void GanttPlanInsertConfirmationAndReplan()
        {

            ProductionDomainContext master40Context = ProductionDomainContext.GetContext(masterCtxString);
            master40Context.CustomerOrders.RemoveRange(master40Context.CustomerOrders);
            master40Context.CustomerOrderParts.RemoveRange(master40Context.CustomerOrderParts);
            master40Context.SaveChanges();

            master40Context.CreateNewOrder(10115, 1, 0, 250);
            master40Context.SaveChanges();

            GanttPlanDBContext ganttPlanContext = GanttPlanDBContext.GetContext(GanttPlanCtxString);

            ganttPlanContext.Database.ExecuteSqlRaw("EXEC sp_MSforeachtable 'DELETE FROM ? '");
            GanttPlanOptRunner.RunOptAndExport("Init");

            Assert.True(ganttPlanContext.GptblProductionorder.Any());

            ganttPlanContext.GptblConfirmation.RemoveRange(ganttPlanContext.GptblConfirmation);
            var productionorder = ganttPlanContext.GptblProductionorder.Single(x => x.ProductionorderId.Equals("000004"));

            ganttPlanContext.GptblConfirmation.RemoveRange(ganttPlanContext.GptblConfirmation);

            var activities = ganttPlanContext.GptblProductionorderOperationActivity.Where(x =>
                x.ProductionorderId.Equals(productionorder.ProductionorderId));

            var activity = activities.Single(x => x.OperationId.Equals("10") && x.ActivityId.Equals(2));
            var confirmation = CreateConfirmation(activity, productionorder, 1);

            ganttPlanContext.GptblConfirmation.Add(confirmation);

            ganttPlanContext.SaveChanges();

            GanttPlanOptRunner.RunOptAndExport("Continuous");

            Assert.True(ganttPlanContext.GptblConfirmation.Any());

            confirmation = ganttPlanContext.GptblConfirmation.SingleOrDefault(x =>
                x.ConfirmationId.Equals(confirmation.ConfirmationId));
            //ganttPlanContext.GptblConfirmation.Remove(confirmation);
            var finishConfirmation = CreateConfirmation(activity, productionorder, 16);
            ganttPlanContext.SaveChanges();

            ganttPlanContext.GptblConfirmation.Add(finishConfirmation);

            activity = activities.Single(x => x.OperationId.Equals("10") && x.ActivityId.Equals(3));

            confirmation = CreateConfirmation(activity, productionorder, 16);

            ganttPlanContext.GptblConfirmation.Add(confirmation);

            activity = activities.Single(x => x.OperationId.Equals("20") && x.ActivityId.Equals(2));

            confirmation = CreateConfirmation(activity, productionorder, 16);
            ganttPlanContext.GptblConfirmation.Add(confirmation);

            ganttPlanContext.SaveChanges();

            GanttPlanOptRunner.RunOptAndExport("Continuous");

            Assert.True(ganttPlanContext.GptblProductionorder.Count().Equals(10));

        }

        private GptblConfirmation CreateConfirmation(GptblProductionorderOperationActivity activity,
            GptblProductionorder productionorder, int confirmationType)
        {
            var newConf = new GptblConfirmation();
            newConf.ProductionorderId = activity.ProductionorderId;
            newConf.ActivityEnd = activity.DateEnd;
            newConf.ActivityStart = activity.DateStart;
            newConf.ClientId = string.Empty;
            newConf.ConfirmationDate = activity.DateEnd;
            newConf.ConfirmationId = Guid.NewGuid().ToString();
            newConf.ProductionorderActivityId = activity.ActivityId;
            newConf.ProductionorderOperationId = activity.OperationId;
            newConf.QuantityFinished = confirmationType == 16 ? productionorder.QuantityNet : 0;
            newConf.QuantityFinishedUnitId = productionorder.QuantityUnitId;
            newConf.ProductionorderSplitId = 0;
            newConf.ConfirmationType = confirmationType; // 16 = beendet
            return newConf;
        }

        [Fact]
        public void TestMaterialRelations()
        {
            using (var localGanttPlanDbContext = GanttPlanDBContext.GetContext(GanttPlanCtxString))
            {

                var materialId = "10115";

                var activities = new Queue<GptblProductionorderOperationActivityResourceInterval>(
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
                        .Where(x => x.ProductionorderOperationActivityResource.ProductionorderOperationActivity
                                        .Productionorder.MaterialId.Equals(materialId)
                                    && x.IntervalAllocationType.Equals(1))
                        .OrderBy(x => x.DateFrom)
                        .ToList());


            } // filter Done and in Progress?
        }
    }
}
