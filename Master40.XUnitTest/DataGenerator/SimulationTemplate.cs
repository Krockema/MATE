﻿using System;
using System.Collections.Generic;
using Master40.DB.Data.Context;
using Master40.Simulation.CLI;
using Master40.SimulationCore;
using Master40.SimulationCore.Environment.Options;
using System.Threading.Tasks;
using AkkaSim.Logging;
using Master40.DataGenerator.Generators;
using Master40.DataGenerator.Repository;
using Microsoft.Extensions.Logging;
using Xunit;
using LogLevel = NLog.LogLevel;
using Master40.SimulationCore.Helper;
using Master40.DB.Nominal;

namespace Master40.XUnitTest.DataGenerator
{
    public class SimulationTemplate
    {
        // local TEST Context
        private const string testCtxString = "Server=(localdb)\\mssqllocaldb;Database=TestContext;Trusted_Connection=True;MultipleActiveResultSets=true";
        private const string testResultCtxString = "Server=(localdb)\\mssqllocaldb;Database=TestResultContext;Trusted_Connection=True;MultipleActiveResultSets=true";
        private const string testGeneratorCtxString = "Server=(localdb)\\mssqllocaldb;Database=TestGeneratorContext;Trusted_Connection=True;MultipleActiveResultSets=true";

        // Definition for Simulation runs each Call returns
        // TODO: return complete config objects to avoid errors, and separate Data Generator / Simulation configurations
        public static IEnumerable<object[]> GetTestData()
        {

            // var simNr = 10000; // Og : 0.98 
            // var simNr = 11000; // Og : 0.0
            //var simNr = 13000; // Og : 0.5
            //var simNr = 14000; // Og : 0.25
            //var simNr = 20000; // Og : 0.75
            var simNr = 131000;
            for (int approach = 30; approach < 35; approach++)
                {

                    for (int i = 0; i < 3; i++)
                    {
                        yield return new object[]
                            {
                            approach // approach id (test data generator input parameter set id)
                            , 1500   // order Quantity
                            , 1920   // max bucket size
                            , 10160    // throughput time
                            , 348345 + i * 13// Random seed
                            , 0.00891 // arrival rate
                            , 30240 // simulation end
                            , 5     // min delivery time
                            , 7     // max delivery time
                            , SimulationType.Default //simulation type
                            , simNr + i  //SimulationNumber
                            };
                    }
                    simNr = simNr + 100;
                }
            }
        

        /// <summary>
        /// To Run this test the Database must have been filled with Master data
        /// </summary>
        /// <param name="approachId"></param>
        /// <param name="orderQuantity"></param>
        /// <param name="maxBucketSize"></param>
        /// <param name="throughput"></param>
        /// <param name="seed"></param>
        /// <param name="arrivalRate"></param>
        /// <param name="simulationEnd"></param>
        /// <param name="minDeliveryTime"></param>
        /// <param name="maxDeliveryTime"></param>
        /// <returns></returns>
        [Theory]
        //[InlineData(SimulationType.DefaultSetup, 1, Int32.MaxValue, 1920, 169, ModelSize.Small, ModelSize.Small)]
        [MemberData(nameof(GetTestData))]
        public async Task SystemTestAsync(int approachId
                                        , int orderQuantity
                                        , int maxBucketSize
                                        , long throughput
                                        , int seed
                                        , double arrivalRate
                                        , long simulationEnd
                                        , int minDeliveryTime
                                        , int maxDeliveryTime
                                        , SimulationType simulationType
                                        , int simulationNumber)
        {
            ResultContext ctxResult = ResultContext.GetContext(resultCon: testResultCtxString);
            ProductionDomainContext masterCtx = ProductionDomainContext.GetContext(testCtxString);
            DataGeneratorContext dataGenCtx = DataGeneratorContext.GetContext(testGeneratorCtxString);

            var approach = ApproachRepository.GetApproachById(dataGenCtx, approachId);
            var generator = new MainGenerator();
            await Task.Run(() =>
                generator.StartGeneration(approach, masterCtx, ctxResult));

            var simContext = new AgentSimulation(DBContext: masterCtx, messageHub: new ConsoleHub());
            var simConfig = ArgumentConverter.ConfigurationConverter(ctxResult, 1);

            //LogConfiguration.LogTo(TargetTypes.Debugger, TargetNames.LOG_AGENTS, LogLevel.Trace, LogLevel.Trace);
            LogConfiguration.LogTo(TargetTypes.Debugger, TargetNames.LOG_AGENTS, LogLevel.Info, LogLevel.Info);
            LogConfiguration.LogTo(TargetTypes.Debugger, TargetNames.LOG_AGENTS, LogLevel.Debug, LogLevel.Debug);
            //LogConfiguration.LogTo(TargetTypes.Debugger, CustomLogger.PRIORITY, LogLevel.Warn, LogLevel.Warn);
            //LogConfiguration.LogTo(TargetTypes.File, CustomLogger.SCHEDULING, LogLevel.Warn, LogLevel.Warn);
            //LogConfiguration.LogTo(TargetTypes.File, CustomLogger.DISPOPRODRELATION, LogLevel.Debug, LogLevel.Debug);
            //LogConfiguration.LogTo(TargetTypes.Debugger, CustomLogger.PROPOSAL, LogLevel.Warn, LogLevel.Warn);
            //LogConfiguration.LogTo(TargetTypes.Debugger, CustomLogger.INITIALIZE, LogLevel.Warn, LogLevel.Warn);
            //LogConfiguration.LogTo(TargetTypes.Debugger, CustomLogger.JOB, LogLevel.Warn, LogLevel.Warn);
            //LogConfiguration.LogTo(TargetTypes.File, CustomLogger.ENQUEUE, LogLevel.Warn, LogLevel.Warn);
            //LogConfiguration.LogTo(TargetTypes.Debugger, CustomLogger.JOBSTATE, LogLevel.Warn, LogLevel.Warn);
            //LogConfiguration.LogTo(TargetTypes.File, TargetNames.LOG_AKKA, LogLevel.Trace, LogLevel.Trace);
            //LogConfiguration.LogTo(TargetTypes.Debugger, TargetNames.LOG_AKKA, LogLevel.Warn);

            var dataGenSim = new DB.GeneratorModel.Simulation();
            dataGenSim.ApproachId = approachId;
            dataGenSim.StartTime = DateTime.Now;
            await Task.Run(() =>
            {
                dataGenCtx.Simulations.AddRange(dataGenSim);
                dataGenCtx.SaveChanges();
            });

            // update customized Configuration
            simConfig.AddOption(new DBConnectionString(testResultCtxString));  
            simConfig.ReplaceOption(new TimeConstraintQueueLength(480));
            simConfig.ReplaceOption(new OrderArrivalRate(value: arrivalRate));
            simConfig.ReplaceOption(new OrderQuantity(value: orderQuantity)); 
            simConfig.ReplaceOption(new EstimatedThroughPut(value: throughput));
            simConfig.ReplaceOption(new TimePeriodForThroughputCalculation(value: 2880));
            simConfig.ReplaceOption(new Seed(value: seed));
            simConfig.ReplaceOption(new SettlingStart(value: 2880));
            simConfig.ReplaceOption(new SimulationKind(value: simulationType));
            simConfig.ReplaceOption(new SimulationEnd(value: simulationEnd));
            simConfig.ReplaceOption(new SaveToDB(value: true));
            simConfig.ReplaceOption(new MaxBucketSize(value: maxBucketSize));
            simConfig.ReplaceOption(new SimulationNumber(value: simulationNumber)); 
            //simConfig.ReplaceOption(new SimulationNumber(value: dataGenSim.Id));
            simConfig.ReplaceOption(new DebugSystem(value: true));
            simConfig.ReplaceOption(new WorkTimeDeviation(0.2));
            // anpassen der Lieferzeiten anhand der Erwarteten Durchlaufzeit. 
            simConfig.ReplaceOption(new MinDeliveryTime(value: minDeliveryTime));
            simConfig.ReplaceOption(new MaxDeliveryTime(value: maxDeliveryTime));
            simConfig.ReplaceOption(new SimulationCore.Environment.Options.PriorityRule(DB.Nominal.PriorityRule.LST));

            await Task.Run(() => 
                ArgumentConverter.ConvertBackAndSave(ctxResult, simConfig, dataGenSim.Id));

            var simulation = await simContext.InitializeSimulation(configuration: simConfig);

            
            if (simulation.IsReady())
            {
                // Start simulation
                var sim = simulation.RunAsync();
                simContext.StateManager.ContinueExecution(simulation);
                await sim;
                dataGenSim.FinishTime = DateTime.Now;
                dataGenSim.FinishedSuccessfully = sim.IsCompletedSuccessfully;
                await Task.Run(() => 
                    dataGenCtx.SaveChanges());
                System.Diagnostics.Debug.WriteLine("################################# Simulation has finished with number " + dataGenSim.Id);
                Assert.True(condition: sim.IsCompleted);
            }
        }

        /* CTE on MasterResultContext (SQL Querry)
         
                DECLARE @simNr BIGINT;
                SET @simNr = 0;

                WITH DirectReports(ProductionOrderId, JobName, Parent,CapabilityProvider, CapabilityName, HirachyNumber, BomLevel) 
                AS
                (       SELECT jobs.ProductionOrderId, jobs.JobName, jobs.ParentId, jobs.CapabilityProvider, jobs.CapabilityName, HierarchyNumber, 0 AS BomLevel
                        FROM SimulationJobs as jobs
		                WHERE jobs.ProductionOrderId = 'ProductionAgent(0)' and jobs.SimulationNumber = @simNr
                        
	                UNION ALL
                        
		                SELECT e.ProductionOrderId, e.JobName, e.Parent, e.CapabilityProvider, e.CapabilityName, e.HierarchyNumber, BomLevel + 1
                        FROM SimulationJobs as e
                        INNER JOIN DirectReports AS dr ON dr.ProductionOrderId = e.Parent and e.SimulationNumber =  @simNr
                )
                    SELECT distinct *
                    FROM DirectReports
	                order by BomLevel, ProductionOrderId ,HirachyNumber

         */

    }
}