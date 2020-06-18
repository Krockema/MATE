using Akka.TestKit.Xunit;
using Akka.Actor;
using Master40.DB.Data.Context;
using Master40.DB.Data.Initializer;
using Master40.DB.Nominal;
using Master40.Simulation.CLI;
using Master40.SimulationCore;
using Master40.SimulationCore.Agents.Guardian;
using Master40.SimulationCore.Environment.Options;
using Master40.Tools.ExtensionMethods;
using Master40.XUnitTest.Online.Preparations;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Internal;
using Xunit;

namespace Master40.XUnitTest.Online.Integration
{
    public class ProductionTest : TestKit
    {

        private ProductionDomainContext _context;
        private string _databaseConnectionString;
        private ResultContext _resultContext;
        private string _resultConnectionString;

        public ProductionTest()
        {
            _databaseConnectionString = Dbms.getDbContextString();
            _context = new ProductionDomainContext(options: new DbContextOptionsBuilder<MasterDBContext>()
                .UseSqlServer(connectionString: _databaseConnectionString)
                .Options);
            _context.Database.EnsureDeleted();
            _context.Database.EnsureCreated();

            _resultConnectionString = Dbms.getResultDatabaseContextString();
            _resultContext = ResultContext.GetContext(resultCon: _resultConnectionString);
            _resultContext.Database.EnsureDeleted();
            _resultContext.Database.EnsureCreated();

        }

        [Theory]
        [InlineData(5, ModelSize.Medium, ModelSize.Small, 0, new []{0,0,0})]
        [InlineData(5, ModelSize.Medium, ModelSize.Small, 2, new[] { 0, 0, 0 })]
        [InlineData(5, ModelSize.Medium, ModelSize.Small, 0, new[] { 1, 1, 1 })]
        [InlineData(5, ModelSize.Medium, ModelSize.Small, 3, new[] { 1, 0, 1 })]

        [InlineData(5, ModelSize.TestModel, ModelSize.Small, 3, new[] { 1, 0, 1 })]
        public async Task RunProduction(int orderQuantity, ModelSize resourceModelSize, ModelSize setupModelSize, int numberOfWorkers, int[] numberOfOperators)
        {
            //Handle this one in our Resource Model?
            var assert = true;
            MasterDBInitializerTruck.DbInitialize(_context, resourceModelSize, setupModelSize, numberOfWorkers, numberOfOperators);
            //InMemoryContext.LoadData(source: _masterDBContext, target: _ctx);

            var simContext = new AgentSimulation(DBContext: _context, messageHub: new ConsoleHub());
            var simConfig = ArgumentConverter.ConfigurationConverter(_resultContext, 1);
            // update customized Items
            simConfig.AddOption(new DBConnectionString(_resultConnectionString));
            simConfig.AddOption(new TimeToAdvance(new TimeSpan(0L)));
            simConfig.AddOption(new KpiTimeSpan(240));
            simConfig.AddOption(new DebugAgents(false));
            simConfig.AddOption(new MinDeliveryTime(1440));
            simConfig.AddOption(new MaxDeliveryTime(2880));
            simConfig.AddOption(new TransitionFactor(3));
            simConfig.ReplaceOption(new SimulationKind(value: SimulationType.Default));
            simConfig.ReplaceOption(new OrderArrivalRate(value: 0.15));
            simConfig.ReplaceOption(new OrderQuantity(value: orderQuantity));
            simConfig.ReplaceOption(new EstimatedThroughPut(value: 1920));
            simConfig.ReplaceOption(new TimePeriodForThroughputCalculation(value: 2880));
            simConfig.ReplaceOption(new Seed(value: 150));
            simConfig.ReplaceOption(new SettlingStart(value: 0));
            simConfig.ReplaceOption(new SimulationEnd(value: 4380));
            simConfig.ReplaceOption(new SaveToDB(value: true));
            simConfig.ReplaceOption(new MaxBucketSize(value: 480));
            simConfig.ReplaceOption(new SimulationNumber(value: 3));
            simConfig.ReplaceOption(new DebugSystem(value: true));
            simConfig.ReplaceOption(new WorkTimeDeviation(0.0));

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
                _resultContext.Kpis
                .Single(x => x.IsFinal.Equals(true) && x.Name.Equals("OrderProcessed")).Value;

            if (processedOrders != orderQuantity)
            {
                assert = false;
            }
            
            //Add more KPIs to check
            
            //Check amount of contract agents
            Assert.True(assert);

        }

    }
}
