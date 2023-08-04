﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Akka.TestKit.Xunit;
using Akka.Util.Internal;
using AkkaSim.Logging;
using Mate.DataCore;
using Mate.DataCore.Data.Context;
using Mate.DataCore.Data.Initializer;
using Mate.DataCore.GanttPlan;
using Mate.DataCore.GanttPlan.GanttPlanModel;
using Mate.DataCore.Nominal;
using Mate.DataCore.Nominal.Model;
using Mate.Production.CLI;
using Mate.Production.Core;
using Mate.Production.Core.Environment.Options;
using Mate.Production.Core.Helper;
using MathNet.Numerics.Distributions;
using Microsoft.EntityFrameworkCore;
using NLog;
using Xunit;

namespace Mate.Test.SimulationEnvironment
{
    public class CentralSystem : TestKit
    {
        private readonly string TestMateDb = "Test" + DataBaseConfiguration.MateDb;
        private readonly string TestMateResultDb = "Test" + DataBaseConfiguration.MateResultDb;

        [Fact]
        public void TestDateUpdate()
        {
            GanttPlanOptRunner.RunOptAndExport("Init");

            var ganttPlanDataBase = Dbms.GetGanttDataBase(DataBaseConfiguration.GP);
            var ganttPlanContext = ganttPlanDataBase.DbContext;

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
        public void ResetGanttPlanDB()
        {
            //Reset GanttPLan DB?
            var ganttPlanContext = Dbms.GetGanttDataBase(DataBaseConfiguration.GP).DbContext;
            ganttPlanContext.Database.ExecuteSqlRaw("EXEC sp_MSforeachtable 'DELETE FROM ? '");
        }

        [Fact]
        //[InlineData(SimulationType.DefaultSetup, 1, Int32.MaxValue, 1920, 169, ModelSize.Small, ModelSize.Small)]
        public async Task CentralSystemTest()
        {
            //LogConfiguration.LogTo(TargetTypes.Debugger, TargetNames.LOG_AGENTS, LogLevel.Trace, LogLevel.Trace);
            LogConfiguration.LogTo(TargetTypes.Debugger, TargetNames.LOG_AGENTS, LogLevel.Info, LogLevel.Info);
            LogConfiguration.LogTo(TargetTypes.Debugger, TargetNames.LOG_AGENTS, LogLevel.Debug, LogLevel.Debug);
            LogConfiguration.LogTo(TargetTypes.Debugger, TargetNames.LOG_AGENTS, LogLevel.Warn, LogLevel.Warn);

            var simtulationType = SimulationType.Central;
            var seed = 169;
            var throughput = 1920;
            var arrivalRate = 0.04;

            //Create Mate Data Base
            //var masterPlanContext = Dbms.GetMateDataBase(dbName: TestMateDb).DbContext;
            //masterPlanContext.Database.EnsureDeleted();
            //masterPlanContext.Database.EnsureCreated();
            //MasterDBInitializerTruck.DbInitialize(masterPlanContext, ModelSize.Medium, ModelSize.Medium, ModelSize.Small, 3, false, false);

            //Create Mate Result Database
            var masterPlanResultContext = Dbms.GetResultDataBase(TestMateResultDb).DbContext;
            masterPlanResultContext.Database.EnsureDeleted();
            masterPlanResultContext.Database.EnsureCreated();
            ResultDBInitializerBasic.DbInitialize(masterPlanResultContext);

            //Reset GanttPLan DB?
            // make sure SimContext has DataBaseConfiguration.GP
            var ganttPlanContext = Dbms.GetGanttDataBase(DataBaseConfiguration.GP); 
            ganttPlanContext.DbContext.Database.ExecuteSqlRaw("EXEC sp_MSforeachtable 'DELETE FROM ? '");

            //Synchronisation GanttPlan
            GanttPlanOptRunner.RunOptAndExport("Init", "C:\\tools\\Ganttplan\\GanttPlanOptRunner.exe");

            var simContext = new GanttSimulation(dbName: TestMateDb, messageHub: new LoggingHub());
            var simConfig = ArgumentConverter.ConfigurationConverter(masterPlanResultContext, 1);
            // update customized Items
            simConfig.AddOption(new ResultsDbConnectionString(masterPlanResultContext.Database.GetConnectionString()));
            simConfig.ReplaceOption(new KpiTimeSpan(240));
            simConfig.ReplaceOption(new TimeConstraintQueueLength(480));
            simConfig.ReplaceOption(new SimulationKind(value: simtulationType));
            simConfig.ReplaceOption(new OrderArrivalRate(value: arrivalRate));
            simConfig.ReplaceOption(new OrderQuantity(value: 10000));
            simConfig.ReplaceOption(new EstimatedThroughPut(value: throughput));
            simConfig.ReplaceOption(new TimePeriodForThroughputCalculation(value: 4000));
            simConfig.ReplaceOption(new Production.Core.Environment.Options.Seed(value: seed));
            simConfig.ReplaceOption(new MinQuantity(value: 1));
            simConfig.ReplaceOption(new MaxQuantity(value: 1));
            simConfig.ReplaceOption(new MinDeliveryTime(value: 4));
            simConfig.ReplaceOption(new MaxDeliveryTime(value: 6));
            simConfig.ReplaceOption(new SettlingStart(value: 60));
            simConfig.ReplaceOption(new SimulationEnd(value: 1440 * 21));
            simConfig.ReplaceOption(new SaveToDB(value: true));
            simConfig.ReplaceOption(new DebugSystem(value: false));
            simConfig.ReplaceOption(new DebugAgents(value: false));
            simConfig.ReplaceOption(new WorkTimeDeviation(0.2));

            var simulation = await simContext.InitializeSimulation(configuration: simConfig);

            ClearResultDBby(simNr: simConfig.GetOption<SimulationNumber>(), dbName: TestMateResultDb);

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
        private void ClearResultDBby(SimulationNumber simNr, string dbName)
        {
            var _simNr = simNr;
            using (var _ctxResult = Dbms.GetResultDataBase(dbName).DbContext)
            {
                var jobsToRemove =
                    _ctxResult.SimulationJobs.Where(predicate: a => a.SimulationNumber.Equals(_simNr.Value)).ToList();
                _ctxResult.RemoveRange(entities: jobsToRemove);
                var tasksToRemove =
                   _ctxResult.TaskItems.Where(predicate: a => a.SimulationNumber.Equals(_simNr.Value)).ToList();
                _ctxResult.RemoveRange(entities: tasksToRemove);
                _ctxResult.RemoveRange(entities: _ctxResult.Kpis.Where(predicate: a => a.SimulationNumber.Equals(_simNr.Value)));
                _ctxResult.RemoveRange(entities: _ctxResult.StockExchanges.Where(predicate: a => a.SimulationNumber.Equals(_simNr.Value)));
                _ctxResult.SaveChanges();
            }
        }


        [Fact(Skip = "Offline")]
        public void AggreteResults()
        {
            var _resultContext = Dbms.GetResultDataBase(TestMateResultDb);

            var aggregator = new ResultAggregator(_resultContext.DbContext);
            aggregator.BuildResults(1);
        }


        [Fact]
        public void TestGanttPlanApi()
        {
           // MateProductionDb mateCtx = Dbms.GetMateDataBase(dbName: TestMateDb).DbContext;

            GanttPlanDBContext ganttPlanContext = Dbms.GetGanttDataBase(DataBaseConfiguration.GP).DbContext;
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
        public void GenerateRandomLogNormal()
        {
            var _sourceRandom = new Random(Seed: 169
                                         //TODO WARUM?
                                         //+ simNumber
                                         );

            var sampler = LogNormal.WithMeanVariance(10, 10 * 0.2, _sourceRandom);
            for (int i = 0; i < 10000; i++)
            {
                
                System.Diagnostics.Debug.WriteLine(Math.Round(sampler.Sample()));
                
            }
        }


        [Fact]
        public void GanttPlanInsertConfirmationAndReplan()
        {
            MateProductionDb mateCtx = Dbms.GetMateDataBase(dbName: TestMateDb).DbContext;
            mateCtx.CustomerOrders.RemoveRange(mateCtx.CustomerOrders);
            mateCtx.CustomerOrderParts.RemoveRange(mateCtx.CustomerOrderParts);
            mateCtx.SaveChanges();
            
            mateCtx.CreateNewOrder(10115, 1, 0, 250);
            mateCtx.SaveChanges();

            GanttPlanDBContext ganttPlanContext = Dbms.GetGanttDataBase(DataBaseConfiguration.GP).DbContext;

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
            using (var localGanttPlanDbContext =  Dbms.GetGanttDataBase(DataBaseConfiguration.GP).DbContext)
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
