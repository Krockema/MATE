using AkkaSim.Logging;
using Master40.DataGenerator.Generators;
using Master40.DataGenerator.Repository;
using Master40.DB;
using Master40.DB.Data.Context;
using Master40.DB.Data.Helper;
using Master40.DB.Nominal;
using Master40.Simulation.CLI;
using Master40.SimulationCore;
using Master40.SimulationCore.Environment.Options;
using Master40.SimulationCore.Types;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;
using LogLevel = NLog.LogLevel;

namespace Master40.XUnitTest.DataGenerator
{
    public class SimulationTemplate
    {
        // Definition for Simulation runs each Call returns
        // TODO: return complete config objects to avoid errors, and separate Data Generator / Simulation configurations
        public static IEnumerable<object[]> GetTestData()
        {
            for (int approach = 1; approach < 2; approach++)
            {
                for (int i = 0; i < 1; i++)
                {
                    yield return new object[]
                    {
                        approach // approach id (test data generator input parameter set id)
                        , 3000   // order Quantity
                        , 960   // max bucket size
                        , 10160    // throughput time
                        , 348345 + i * 14// Random seed
                        , 0.04 // arrival rate
                        , 10080*1 // simulation end
                        , 10     // min delivery time
                        , 15     // max delivery time
                        , SimulationType.Default //simulation type
                        , int.Parse(5.ToString() + approach.ToString().PadLeft(3, '0')
                                                 + i.ToString().PadLeft(2, '0'))  //SimulationNumber
                    };
                }
            }


            /*for (int approach = 1; approach < 10; approach++)
            {
                for (int i = 0; i < 10; i++)
                {
                    yield return new object[]
                    {
                        approach // approach id (test data generator input parameter set id)
                        , 3000   // order Quantity
                        , 960   // max bucket size
                        , 10160    // throughput time
                        , 348345 + i * 14// Random seed
                        , 0.04 // arrival rate
                        , 10080*3 // simulation end
                        , 10     // min delivery time
                        , 15     // max delivery time
                        , SimulationType.Queuing //simulation type
                        , int.Parse(2.ToString() + approach.ToString().PadLeft(3, '0')
                                    + i.ToString().PadLeft(2, '0'))  //SimulationNumber
                    };
                }
              }*/
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
            var mainDbName = "Test";
            DataBase<ResultContext> DbResult = Dbms.GetResultDataBase(dbName: $"{mainDbName}ResultContext");
            DataBase<ProductionDomainContext> DbMasterCtx = Dbms.GetMasterDataBase(dbName: mainDbName);
            DataBase<DataGeneratorContext> DbGenerator = Dbms.GetGeneratorDataBase(dbName: "TestGeneratorContext");

            var approach = ApproachRepository.GetApproachById(DbGenerator.DbContext, approachId);
            var generator = new MainGenerator();
            await Task.Run(() =>
                generator.StartGeneration(approach, DbMasterCtx.DbContext, DbResult.DbContext));


            var simConfig = ArgumentConverter.ConfigurationConverter(DbResult.DbContext, 1);

            //LogConfiguration.LogTo(TargetTypes.Debugger, TargetNames.LOG_AGENTS, LogLevel.Trace, LogLevel.Trace);
            LogConfiguration.LogTo(TargetTypes.Debugger, TargetNames.LOG_AGENTS, LogLevel.Info, LogLevel.Info);
            //LogConfiguration.LogTo(TargetTypes.Debugger, TargetNames.LOG_AGENTS, LogLevel.Debug, LogLevel.Debug);
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

            DbGenerator.DbContext.Simulations.AddRange(dataGenSim);
            DbGenerator.DbContext.SaveChanges();

            // update customized Configuration
            simConfig.AddOption(new ResultsDbConnectionString(DbResult.ConnectionString.Value));
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
            simConfig.ReplaceOption(new DebugAgents(value: true));
            simConfig.ReplaceOption(new WorkTimeDeviation(0.2));
            // anpassen der Lieferzeiten anhand der Erwarteten Durchlaufzeit. 
            simConfig.ReplaceOption(new MinDeliveryTime(value: minDeliveryTime));
            simConfig.ReplaceOption(new MaxDeliveryTime(value: maxDeliveryTime));
            simConfig.ReplaceOption(new SimulationCore.Environment.Options.PriorityRule(DB.Nominal.PriorityRule.LST));

            ArgumentConverter.ConvertBackAndSave(DbResult.DbContext, simConfig, dataGenSim.Id);

            ISimulation simContext =
             simulationType.Equals(SimulationType.Central) ?
             /*then*/   new GanttSimulation(mainDbName, messageHub: new ConsoleHub())
             /*else*/   : new AgentSimulation(mainDbName, messageHub: new ConsoleHub());

            var simulation = await simContext.InitializeSimulation(configuration: simConfig);

            if (simulation.IsReady())
            {
                // Start simulation
                var sim = simulation.RunAsync();
                simContext.StateManager.ContinueExecution(simulation);
                await sim;
                dataGenSim.FinishTime = DateTime.Now;
                dataGenSim.FinishedSuccessfully = sim.IsCompletedSuccessfully;
                DbGenerator.DbContext.SaveChanges();
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