using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Akka.TestKit.Xunit;
using AkkaSim.Logging;
using Mate.DataCore;
using Mate.DataCore.Data.Context;
using Mate.DataCore.Data.Helper;
using Mate.DataCore.Data.Helper.Types;
using Mate.DataCore.Data.Initializer;
using Mate.DataCore.DataModel;
using Mate.DataCore.GanttPlan;
using Mate.DataCore.Nominal;
using Mate.DataCore.Nominal.Model;
using Mate.Production.CLI;
using Mate.Production.Core;
using Mate.Production.Core.Environment.Options;
using Mate.Production.Core.Helper;
using Microsoft.EntityFrameworkCore;
using NLog;
using Xunit;
using PriorityRule = Mate.DataCore.Nominal.PriorityRule;

namespace Mate.Test.SimulationEnvironment
{
    public class AgentSystem : TestKit, IClassFixture<SeedInitializer>
    {
        private SeedInitializer seedInitializer = new SeedInitializer();

        private readonly string TestMateDb = "Test" + DataBaseConfiguration.MateDb;
        private readonly string TestMateResultDb = "Test" + DataBaseConfiguration.MateResultDb;


        [Fact]
        public void TestRawSQL()
        {
            
            string sql = string.Format(
                @"CREATE OR ALTER PROCEDURE ArticleCTE
	            @ArticleId int
                AS
                BEGIN
	                SET NOCOUNT ON;
	                DROP TABLE IF EXISTS dbo.#Temp;
	                DROP TABLE IF EXISTS dbo.#Union;

	                WITH Parts(AssemblyID, ComponentID, PerAssemblyQty, ComponentLevel) AS  
	                (  
		                SELECT b.ArticleParentId, b.ArticleChildId, CAST(b.Quantity AS decimal),0 AS ComponentLevel  
		                FROM dbo.M_ArticleBom  AS b  
		                join dbo.M_Article a on a.Id = b.ArticleParentId
		                where @ArticleId = a.Id
		                UNION ALL  
		                SELECT bom.ArticleParentId, bom.ArticleChildId, CAST(PerAssemblyQty * bom.Quantity as DECIMAL), ComponentLevel + 1  
		                 FROM dbo.M_ArticleBom  AS bom  
			                INNER join dbo.M_Article ac on ac.Id = bom.ArticleParentId
			                INNER JOIN Parts AS p ON bom.ArticleParentId = p.ComponentID  
	                )

	                select * into #Temp 
	                from (
		                select pr.Id,pr.Name, Sum(p.PerAssemblyQty) as qty, pr.ToBuild as ToBuild
		                FROM Parts AS p INNER JOIN M_Article AS pr ON p.ComponentID = pr.Id
		                Group By pr.Id, pr.Name, p.ComponentID, pr.ToBuild) as x

	                select * into #Union from (
		                select Sum(o.Duration * t.qty) as dur, sum(t.qty) as count ,0 as 'Po'
			                from dbo.M_Operation o join #Temp t on t.Id = o.ArticleId
			                where o.ArticleId in (select t.Id from #Temp t)
	                UNION ALL
		                SELECT SUM(ot.Duration) as dur, COUNT(*) as count , 0 as 'Po'
			                from dbo.M_Operation ot where ot.ArticleId = @ArticleId ) as x
	                UNION ALL 
		                SELECT 0 as dur, 0 as count, sum(t.qty) + 1 as 'Po'
		                from #Temp t where t.ToBuild = 1
	                select Sum(u.dur) as SumDuration , sum(u.count) as SumOperations, sum(u.Po)  as ProductionOrders from #Union u
                END");

            using (var command = Dbms.GetMateDataBase(dbName: TestMateDb).DbContext.Database.GetDbConnection().CreateCommand())
            {
                command.CommandText = sql;
                Dbms.GetMateDataBase(dbName: TestMateDb).DbContext.Database.OpenConnection();
                command.ExecuteNonQuery();
            }

        }

        [Fact]
        public void ReadRawSQL()
        {
            var articleId = 63380;
            var sql = string.Format("Execute ArticleCTE {0}", articleId);
            using (var command = Dbms.GetMateDataBase(dbName: TestMateDb).DbContext.Database.GetDbConnection().CreateCommand())
            {
                command.CommandText = sql;
                Dbms.GetMateDataBase(dbName: "Test").DbContext.Database.OpenConnection();
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        System.Diagnostics.Debug.WriteLine(string.Format("Summe der Dauer {0}; Summe der Operationen {1}; Summe der Prodktionsauftr�ge {2}", reader[0], reader[1], reader[2]));
                    }

                }
            }

        }

        // [Fact(Skip = "MANUAL USE ONLY --> to reset Remote DB")]
        [Fact]
        public void ResetAllDatabase()
        {
            MateResultDb results = Dbms.GetResultDataBase(DataBaseConfiguration.MateResultDb).DbContext;
            results.Database.EnsureDeleted();
            results.Database.EnsureCreated();
            ResultDBInitializerBasic.DbInitialize(results);

            MateDb masterCtx = Dbms.GetMateDataBase(dbName: DataBaseConfiguration.MateDb).DbContext;
            masterCtx.Database.EnsureDeleted();
            masterCtx.Database.EnsureCreated();
            MasterDBInitializerTruck.DbInitialize(masterCtx, resourceModelSize: ModelSize.Large, setupModelSize: ModelSize.Small, ModelSize.Small, 3, false, false);

            HangfireDBContext dbContext = Dbms.GetHangfireDataBase(DataBaseConfiguration.MateHangfireDb).DbContext;
            dbContext.Database.EnsureDeleted();
            dbContext.Database.EnsureCreated();
            HangfireDBInitializer.DbInitialize(context: dbContext);
        }

        [Fact(Skip = "MANUAL USE ONLY --> to reset Remote DB")]
        public void ClearHangfire()
        {
            HangfireDBContext dbContext = Dbms.GetHangfireDataBase("Hangfire").DbContext;
            dbContext.Database.EnsureDeleted();
            dbContext.Database.EnsureCreated();
            HangfireDBInitializer.DbInitialize(context: dbContext);
        }

        [Fact]
        public void SomethingToPlayWith()
        {
            var masterCtx = Dbms.GetMateDataBase(dbName: TestMateDb).DbContext;
            var resources = masterCtx.Resources
                //.Where(x => x.Count == 1)
                // .Include(x => x.RequiresResourceSetups)
                //     .ThenInclude(x => x.ChildResource)
                // .Include(x => x.UsedInResourceSetups)
                //     .ThenInclude(x => x.ResourceCapability)
                .ToList(); // all Resources

            foreach (var resource in resources)
            {
                GetSetups(resource, masterCtx);
            }

        }
        
        private void GetSetups(M_Resource resource, MateProductionDb masterCtx)
        {
            if (!resource.IsPhysical)
                return;
            var setups = masterCtx.ResourceSetups
                .Include(x => x.ResourceCapabilityProvider)
                    .ThenInclude(x => x.ResourceCapability)
                .Include(x => x.Resource)
                .Where(x => x.ResourceId == resource.Id).ToList();
            
            System.Diagnostics.Debug.WriteLine($"Creating Resource: {resource.Name} with following setups...");
            foreach (var setup in setups)
            {
                System.Diagnostics.Debug.WriteLine($"{setup.Name} : {setup.ResourceCapabilityProvider.Name} : {setup.ResourceCapabilityProviderId}");
            }
        }


        // [InlineData(SimulationType.Default, 700, 480, 1920, 594, ModelSize.Medium, ModelSize.Medium, 0.015, false, false)]
        // [InlineData(SimulationType.Default, 701, 480, 1920, 281, ModelSize.Medium, ModelSize.Medium, 0.015, false, false)]
        // [InlineData(SimulationType.Default, 702, 480, 1920, 213, ModelSize.Medium, ModelSize.Medium, 0.015, false, false)]
        // [InlineData(SimulationType.Default, 703, 480, 1920, 945, ModelSize.Medium, ModelSize.Medium, 0.015, false, false)]
        // [InlineData(SimulationType.Default, 704, 480, 1920, 998, ModelSize.Medium, ModelSize.Medium, 0.015, false, false)]
        // [InlineData(SimulationType.Default, 705, 480, 1920, 120, ModelSize.Medium, ModelSize.Medium, 0.015, false, false)]
        // [InlineData(SimulationType.Default, 706, 480, 1920, 124, ModelSize.Medium, ModelSize.Medium, 0.015, false, false)]
        // [InlineData(SimulationType.Default, 707, 480, 1920, 854, ModelSize.Medium, ModelSize.Medium, 0.015, false, false)]
        // [InlineData(SimulationType.Default, 708, 480, 1920, 213, ModelSize.Medium, ModelSize.Medium, 0.015, false, false)]
        // [InlineData(SimulationType.Default, 2, 480, 1920, 325, ModelSize.Medium, ModelSize.Medium, 0.015, false, false)]

        public static IEnumerable<object[]> GetTestData()
        {
            var simNumber = 0;
            var throughput = 1920;

            for (int i = 0; i < 1; i++)
            {
                yield return new object[]
                {
                    SimulationType.Default, //simulationType
                    PriorityRule.LST, //priorityRule
                    simNumber++, 
                    960, 
                    throughput, 
                    594, 
                    ModelSize.Medium, 
                    ModelSize.Medium, 
                    0.0153, 
                    false, 
                    false
                };
                throughput += 100;
            }
        }

        //[Theory]

        //[MemberData(nameof(GetTestData))]
        //public async Task SystemTestAsync(SimulationType simulationType, PriorityRule priorityRule, int simNr, int maxBucketSize, long throughput, int seed , ModelSize resourceModelSize, ModelSize setupModelSize, double arrivalRate, bool distributeSetupsExponentially, bool createMeasurements = false)
        
        [Theory]
        [InlineData(SimulationType.Default, 160, 0.0)]
        [InlineData(SimulationType.Default, 161, 0.1)]
        [InlineData(SimulationType.Default, 162, 0.2)]
        [InlineData(SimulationType.Default, 163, 0.3)]
        [InlineData(SimulationType.Central, 260, 0.0)]
        [InlineData(SimulationType.Central, 261, 0.1)]
        [InlineData(SimulationType.Central, 262, 0.2)]
        [InlineData(SimulationType.Central, 263, 0.3)]
        public async Task AgentSystemTest(SimulationType simulationType, int simNr, double deviation)
        {
            //var simNr = 2;
            //var simulationType = SimulationType.Default;
            var seed = 169;
            var throughput = 1920;
            var arrivalRate = 0.035;

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
            //LogConfiguration.LogTo(TargetTypes.File, CustomLogger.JOBSTATE, LogLevel.Warn, LogLevel.Warn);
            //LogConfiguration.LogTo(TargetTypes.File, TargetNames.LOG_AKKA, LogLevel.Trace, LogLevel.Trace);
            //LogConfiguration.LogTo(TargetTypes.Debugger, TargetNames.LOG_AKKA, LogLevel.Warn);

            //Create Result Context
            var dbResult = Dbms.GetResultDataBase(TestMateResultDb);
            //dbResult.DbContext.Database.EnsureDeleted();
            //dbResult.DbContext.Database.EnsureCreated();
            //ResultDBInitializerBasic.DbInitialize(dbResult.DbContext);

            seedInitializer.GenerateTestData(TestMateDb);
            
            //dbMaster.DbContext.Database.EnsureDeleted();
            //dbMaster.DbContext.Database.EnsureCreated();
            //MasterDBInitializerTruck.DbInitialize(context: dbMaster.DbContext
            //    , resourceModelSize: resourceModelSize
            //    , setupModelSize: setupModelSize
            //    , operatorsModelSize: ModelSize.Small
            //    , numberOfWorkersForProcessing: 3
            //    , secondResource: false
            //    , createMeasurements: createMeasurements
            //    , distributeSetupsExponentially: distributeSetupsExponentially);
            //InMemoryContext.LoadData(source: _masterDBContext, target: _ctx);

            var simConfig = Production.CLI.ArgumentConverter.ConfigurationConverter(dbResult.DbContext, 1);
            // update customized Items
            simConfig.AddOption(new ResultsDbConnectionString(dbResult.ConnectionString.Value));
            simConfig.ReplaceOption(new KpiTimeSpan(240));
            simConfig.ReplaceOption(new TimeConstraintQueueLength(480));
            simConfig.ReplaceOption(new SimulationKind(value: simulationType));
            simConfig.ReplaceOption(new OrderArrivalRate(value: arrivalRate));
            simConfig.ReplaceOption(new OrderQuantity(value: 10000));
            simConfig.ReplaceOption(new EstimatedThroughPut(value: throughput));
            simConfig.ReplaceOption(new TimePeriodForThroughputCalculation(value: 4000));
            simConfig.ReplaceOption(new Production.Core.Environment.Options.Seed(value: seed));
            simConfig.ReplaceOption(new SettlingStart(value: 2880));
            simConfig.ReplaceOption(new MinDeliveryTime(value: 10));
            simConfig.ReplaceOption(new MaxDeliveryTime(value: 15));
            simConfig.ReplaceOption(new SimulationEnd(value: 10080));
            simConfig.ReplaceOption(new SaveToDB(value: true));
            simConfig.ReplaceOption(new DebugSystem(value: false));
            simConfig.ReplaceOption(new DebugAgents(value: false));
            simConfig.ReplaceOption(new WorkTimeDeviation(deviation));
            simConfig.ReplaceOption(new MaxBucketSize(value: 1920));
            simConfig.ReplaceOption(new SimulationNumber(value: simNr));
            simConfig.ReplaceOption(new CreateQualityData(false));
            simConfig.ReplaceOption(new Mate.Production.Core.Environment.Options.PriorityRule(PriorityRule.LST));

            ClearResultDBby(simNr: simConfig.GetOption<SimulationNumber>(), dbName: TestMateResultDb);

            BaseSimulation simContext = null;

            if (simulationType == SimulationType.Default)
            {
                simContext = new AgentSimulation(TestMateDb, messageHub: new ConsoleHub());
            }
            else
            {
                var ganttPlanContext = Dbms.GetGanttDataBase(DataBaseConfiguration.GP);
                ganttPlanContext.DbContext.Database.ExecuteSqlRaw("EXEC sp_MSforeachtable 'DELETE FROM ? '");

                //Synchronisation GanttPlan
                GanttPlanOptRunner.RunOptAndExport("Init", "D:\\Work\\GANTTPLAN\\GanttPlanOptRunner.exe");

                simContext = new GanttSimulation(dbName: TestMateDb, messageHub: new ConsoleHub());

            }

            var simulation = await simContext.InitializeSimulation(configuration: simConfig);

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

        [Fact (Skip = "Offline")]
        public void AggreteResults()
        {
            var _resultContext = Dbms.GetResultDataBase(TestMateResultDb).DbContext;

            var aggregator = new ResultAggregator(_resultContext);
            aggregator.BuildResults(1);
        }
        
        
        [Fact]
        private void ArgumentConverter()
        {
             var dbResult = Dbms.GetResultDataBase(TestMateResultDb);
            var numberOfArguments = dbResult.DbContext.ConfigurationRelations.Count(x => x.Id == 1);
            var config = Production.CLI.ArgumentConverter.ConfigurationConverter(dbResult.DbContext, 2);
            Assert.Equal(numberOfArguments + 1, config.Count());
        }

        private void ClearResultDBby(SimulationNumber simNr, string dbName)
        {
            var _simNr = simNr;
            using (var _ctxResult = Dbms.GetResultDataBase(dbName).DbContext)
            {
                var itemsToRemove =
                    _ctxResult.SimulationJobs.Where(predicate: a => a.SimulationNumber.Equals(_simNr.Value)).ToList();
                _ctxResult.RemoveRange(entities: itemsToRemove);
                _ctxResult.RemoveRange(entities: _ctxResult.Kpis.Where(predicate: a => a.SimulationNumber.Equals(_simNr.Value)));
                _ctxResult.RemoveRange(entities: _ctxResult.StockExchanges.Where(predicate: a => a.SimulationNumber.Equals(_simNr.Value)));
                _ctxResult.SaveChanges();
            }
        }


        [Fact]
        private void TestDbms()
        {
            var dataBaseName = new DataBaseName(TestMateDb);
            var connectionString = Constants.CreateServerConnectionString(dataBaseName);
            System.Diagnostics.Debug.WriteLine(connectionString);
            Assert.NotNull(connectionString);
        }
    }
}
