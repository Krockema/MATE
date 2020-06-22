using Akka.TestKit.Xunit;
using Master40.DB.Data.Context;
using Master40.DB.Data.Helper;
using Master40.DB.Data.Initializer;
using Master40.DB.Nominal;
using Master40.Simulation.CLI;
using Master40.SimulationCore;
using Master40.SimulationCore.Environment.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Master40.XUnitTest.Online.Integration
{
    public class ProductionTest : TestKit
    {
        private DataBase<ProductionDomainContext> _contextDataBase;
        private DataBase<ResultContext> _resultContextDataBase;

        public ProductionTest()
        {
            _contextDataBase = DB.Dbms.GetNewMasterDataBase();
            _resultContextDataBase = DB.Dbms.GetNewResultDataBase();
            
        }

        /// <summary>
        /// Model is limited to three groups atm
        /// </summary>
        /// <param name="orderQuantity"></param>
        /// <param name="resourceModelSize"></param>
        /// <param name="setupModelSize"></param>
        /// <param name="numberOfWorkers"></param>
        /// <param name="numberOfOperators"></param>
        /// <param name="secondResource"></param>
        /// <returns></returns>
        [Theory]
        [InlineData(1, 5, ModelSize.Medium, ModelSize.Small, 0, new[] { 0, 0, 0 }, false)]
        [InlineData(2, 5, ModelSize.Medium, ModelSize.Small, 2, new[] { 0, 0, 0 }, false)]
        [InlineData(3, 5, ModelSize.Medium, ModelSize.Small, 0, new[] { 1, 1, 1 }, false)]
        [InlineData(4, 5, ModelSize.Medium, ModelSize.Small, 3, new[] { 1, 0, 1 }, false)]

        [InlineData(5, 5, ModelSize.Medium, ModelSize.Small, 0, new[] { 1, 0, 1 }, false)]
        public async Task RunProduction(int uniqueSimNum, int orderQuantity, ModelSize resourceModelSize, ModelSize setupModelSize, int numberOfWorkers, int[] numberOfOperators, bool secondResource)
        {
            //Handle this one in our Resource Model?
            var assert = true;
            MasterDBInitializerTruck.DbInitialize(_contextDataBase.DbContext, resourceModelSize, setupModelSize, numberOfWorkers, numberOfOperators, secondResource);

            ResultDBInitializerBasic.DbInitialize(_resultContextDataBase.DbContext);

            var simContext = new AgentSimulation(DBContext: _contextDataBase.DbContext, messageHub: new ConsoleHub());
            var simConfig = ArgumentConverter.ConfigurationConverter(_resultContextDataBase.DbContext, 1);

            simConfig.AddOption(new DBConnectionString(_resultContextDataBase.ConnectionString.Value));
            simConfig.AddOption(new TimeToAdvance(new TimeSpan(0L)));
            simConfig.AddOption(new KpiTimeSpan(240));
            simConfig.AddOption(new DebugAgents(false));
            simConfig.AddOption(new MinDeliveryTime(1440));
            simConfig.AddOption(new MaxDeliveryTime(2880));
            simConfig.AddOption(new TransitionFactor(3));
            simConfig.ReplaceOption(new SimulationKind(value: SimulationType.Default));
            simConfig.ReplaceOption(new DebugSystem(false));
            simConfig.ReplaceOption(new OrderArrivalRate(value: 0.15));
            simConfig.ReplaceOption(new OrderQuantity(value: orderQuantity));
            simConfig.ReplaceOption(new EstimatedThroughPut(value: 1920));
            simConfig.ReplaceOption(new TimePeriodForThroughputCalculation(value: 2880));
            simConfig.ReplaceOption(new Seed(value: 150));
            simConfig.ReplaceOption(new SettlingStart(value: 0));
            simConfig.ReplaceOption(new SimulationEnd(value: 4380));
            simConfig.ReplaceOption(new SaveToDB(value: true));
            simConfig.ReplaceOption(new MaxBucketSize(value: 480));
            simConfig.ReplaceOption(new SimulationNumber(value: uniqueSimNum));
            simConfig.ReplaceOption(new DebugSystem(value: true));
            simConfig.ReplaceOption(new WorkTimeDeviation(0.0));
            simConfig.ReplaceOption(new TimeConstraintQueueLength(480));

            var simulation = await simContext.InitializeSimulation(configuration: simConfig);

            var sim = simulation.RunAsync();
            simContext.StateManager.ContinueExecution(simulation);


            var taskException = Record.ExceptionAsync(async () => await sim);
            if (taskException.IsCanceled || taskException.Exception != null)
            {
                assert = false;
            }

            // What to check?
            // Any Exception thrown? 
            // all contracts terminated ? 
            // all Production / Job Agents finished ? 

            var processedOrders = 
                _resultContextDataBase.DbContext.Kpis
                .Single(x => x.IsFinal.Equals(true) && x.Name.Equals("OrderProcessed")).Value;

            if (processedOrders != orderQuantity)
            {
                assert = false;
            }

            _contextDataBase.DbContext.Dispose();

            _resultContextDataBase.DbContext.Dispose();
            //Add more KPIs to check

            //Check amount of contract agents
            Assert.True(assert);

        }

    }
}
