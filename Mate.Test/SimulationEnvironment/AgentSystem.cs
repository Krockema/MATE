using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;
using Akka.TestKit.Xunit;
using AkkaSim.Logging;
using Mate.DataCore;
using Mate.DataCore.Data.Context;
using Mate.DataCore.Data.Helper;
using Mate.DataCore.Data.Helper.Types;
using Mate.DataCore.Data.Initializer;
using Mate.DataCore.Data.WrappersForPrimitives;
using Mate.DataCore.DataModel;
using Mate.DataCore.GanttPlan;
using Mate.DataCore.Nominal;
using Mate.DataCore.Nominal.Model;
using Mate.Production.CLI;
using Mate.Production.Core;
using Mate.Production.Core.Agents.CollectorAgent.Types;
using Mate.Production.Core.Environment.Options;
using Mate.Production.Core.Helper;
using MathNet.Numerics.Distributions;
using Microsoft.EntityFrameworkCore;
using NLog;
using Xunit;
using PriorityRule = Mate.DataCore.Nominal.PriorityRule;

namespace Mate.Test.SimulationEnvironment
{
    public class AgentSystem : TestKit, IClassFixture<SeedInitializer>
    {
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

        [Fact]
        public void TestStatistics()
        {
            StabilityManager.Instance.ReadFile();

            Assert.True(true, "yes");
        }

        // [Fact(Skip = "MANUAL USE ONLY --> to reset Remote DB")]
        [Fact]
        public void ResetAllDatabase()
        {
            MateResultDb results = Dbms.GetResultDataBase(TestMateResultDb).DbContext;
            results.Database.EnsureDeleted();
            results.Database.EnsureCreated();
            ResultDBInitializerBasic.DbInitialize(results);

            MateDb masterCtx = Dbms.GetMateDataBase(dbName: TestMateDb).DbContext;
            masterCtx.Database.EnsureDeleted();
            masterCtx.Database.EnsureCreated();
            MasterDBInitializerTruck.DbInitialize(masterCtx, resourceModelSize: ModelSize.Large, setupModelSize: ModelSize.Small, ModelSize.Small, 3, false, false);

            //HangfireDBContext dbContext = Dbms.GetHangfireDataBase(DataBaseConfiguration.MateHangfireDb).DbContext;
            //dbContext.Database.EnsureDeleted();
            //dbContext.Database.EnsureCreated();
            //HangfireDBInitializer.DbInitialize(context: dbContext);
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

        [Fact]
        private void GeneratorForWorkdeviations()
        {
            string path = @"D:\Work\Stability\MyTest2.csv";
            double _deviation = 0.2;
            int duration = 10;
            var listOfInts = new List<int>();
            for (int i = 0; i < 100000; i++)
            {

                var _sourceRandom = new Random(Seed: Guid.NewGuid().GetHashCode());
                listOfInts.Add((int)Math.Round(LogNormal.WithMeanVariance(duration, duration * _deviation, _sourceRandom).Sample()));

            }

            if (!System.IO.File.Exists(path))
            {
                System.IO.File.WriteAllLines(path, listOfInts.Select(x => string.Join(",", x)));
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
            var throughput = 10080;

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
        // [x] [InlineData(SimulationType.Default, 110, 0.00, 0.035, 1337)] // throughput dynamic ruled
        [InlineData(SimulationType.Default, 91000, 0.00, 0.035, 169)]
        [InlineData(SimulationType.Default, 91010, 0.10, 0.035, 169)]
        [InlineData(SimulationType.Default, 91020, 0.15, 0.035, 169)]
        [InlineData(SimulationType.Default, 91030, 0.20, 0.035, 169)]
        [InlineData(SimulationType.Default, 91040, 0.25, 0.035, 169)]
        [InlineData(SimulationType.Default, 91050, 0.30, 0.035, 169)]
        [InlineData(SimulationType.Default, 91060, 0.35, 0.035, 169)]
                                            
        [InlineData(SimulationType.Default, 91001, 0.00, 0.035, 169)]
        [InlineData(SimulationType.Default, 91011, 0.10, 0.035, 169)]
        [InlineData(SimulationType.Default, 91021, 0.15, 0.035, 169)]
        [InlineData(SimulationType.Default, 91031, 0.20, 0.035, 169)]
        [InlineData(SimulationType.Default, 91041, 0.25, 0.035, 169)]
        [InlineData(SimulationType.Default, 91051, 0.30, 0.035, 169)]
        [InlineData(SimulationType.Default, 91061, 0.35, 0.035, 169)]
                                            
        [InlineData(SimulationType.Default, 91002, 0.00, 0.035, 169)]
        [InlineData(SimulationType.Default, 91012, 0.10, 0.035, 169)]
        [InlineData(SimulationType.Default, 91022, 0.15, 0.035, 169)]
        [InlineData(SimulationType.Default, 91032, 0.20, 0.035, 169)]
        [InlineData(SimulationType.Default, 91042, 0.25, 0.035, 169)]
        [InlineData(SimulationType.Default, 91052, 0.30, 0.035, 169)]
        [InlineData(SimulationType.Default, 91062, 0.35, 0.035, 169)]
                                            
        [InlineData(SimulationType.Default, 91003, 0.00, 0.035, 169)]
        [InlineData(SimulationType.Default, 91013, 0.10, 0.035, 169)]
        [InlineData(SimulationType.Default, 91023, 0.15, 0.035, 169)]
        [InlineData(SimulationType.Default, 91033, 0.20, 0.035, 169)]
        [InlineData(SimulationType.Default, 91043, 0.25, 0.035, 169)]
        [InlineData(SimulationType.Default, 91053, 0.30, 0.035, 169)]
        [InlineData(SimulationType.Default, 91063, 0.35, 0.035, 169)]
                                            
        [InlineData(SimulationType.Default, 91004, 0.00, 0.035, 169)]
        [InlineData(SimulationType.Default, 91014, 0.10, 0.035, 169)]
        [InlineData(SimulationType.Default, 91024, 0.15, 0.035, 169)]
        [InlineData(SimulationType.Default, 91034, 0.20, 0.035, 169)]
        [InlineData(SimulationType.Default, 91044, 0.25, 0.035, 169)]
        [InlineData(SimulationType.Default, 91054, 0.30, 0.035, 169)]
        [InlineData(SimulationType.Default, 91064, 0.35, 0.035, 169)]
                                            
        [InlineData(SimulationType.Default, 91005, 0.00, 0.035, 169)]
        [InlineData(SimulationType.Default, 91015, 0.10, 0.035, 169)]
        [InlineData(SimulationType.Default, 91025, 0.15, 0.035, 169)]
        [InlineData(SimulationType.Default, 91035, 0.20, 0.035, 169)]
        [InlineData(SimulationType.Default, 91045, 0.25, 0.035, 169)]
        [InlineData(SimulationType.Default, 91055, 0.30, 0.035, 169)]
        [InlineData(SimulationType.Default, 91065, 0.35, 0.035, 169)]
                                            
        [InlineData(SimulationType.Default, 91006, 0.00, 0.035, 169)]
        [InlineData(SimulationType.Default, 91016, 0.10, 0.035, 169)]
        [InlineData(SimulationType.Default, 91026, 0.15, 0.035, 169)]
        [InlineData(SimulationType.Default, 91036, 0.20, 0.035, 169)]
        [InlineData(SimulationType.Default, 91046, 0.25, 0.035, 169)]
        [InlineData(SimulationType.Default, 91056, 0.30, 0.035, 169)]
        [InlineData(SimulationType.Default, 91066, 0.35, 0.035, 169)]
                                            
        [InlineData(SimulationType.Default, 91007, 0.00, 0.035, 169)]
        [InlineData(SimulationType.Default, 91017, 0.10, 0.035, 169)]
        [InlineData(SimulationType.Default, 91027, 0.15, 0.035, 169)]
        [InlineData(SimulationType.Default, 91037, 0.20, 0.035, 169)]
        [InlineData(SimulationType.Default, 91047, 0.25, 0.035, 169)]
        [InlineData(SimulationType.Default, 91057, 0.30, 0.035, 169)]
        [InlineData(SimulationType.Default, 91067, 0.35, 0.035, 169)]
                                            
        [InlineData(SimulationType.Default, 91008, 0.00, 0.035, 169)]
        [InlineData(SimulationType.Default, 91018, 0.10, 0.035, 169)]
        [InlineData(SimulationType.Default, 91028, 0.15, 0.035, 169)]
        [InlineData(SimulationType.Default, 91038, 0.20, 0.035, 169)]
        [InlineData(SimulationType.Default, 91048, 0.25, 0.035, 169)]
        [InlineData(SimulationType.Default, 91058, 0.30, 0.035, 169)]
        [InlineData(SimulationType.Default, 91068, 0.35, 0.035, 169)]
                                            
        [InlineData(SimulationType.Default, 91009, 0.00, 0.035, 169)]
        [InlineData(SimulationType.Default, 91019, 0.10, 0.035, 169)]
        [InlineData(SimulationType.Default, 91029, 0.15, 0.035, 169)]
        [InlineData(SimulationType.Default, 91039, 0.20, 0.035, 169)]
        [InlineData(SimulationType.Default, 91049, 0.25, 0.035, 169)]
        [InlineData(SimulationType.Default, 91059, 0.30, 0.035, 169)]
        [InlineData(SimulationType.Default, 91069, 0.35, 0.035, 169)]
                                            
        [InlineData(SimulationType.Queuing, 91000, 0.00, 0.035, 169)]
        [InlineData(SimulationType.Queuing, 91010, 0.10, 0.035, 169)]
        [InlineData(SimulationType.Queuing, 91020, 0.15, 0.035, 169)]
        [InlineData(SimulationType.Queuing, 91030, 0.20, 0.035, 169)]
        [InlineData(SimulationType.Queuing, 91040, 0.25, 0.035, 169)]
        [InlineData(SimulationType.Queuing, 91050, 0.30, 0.035, 169)]
        [InlineData(SimulationType.Queuing, 91060, 0.35, 0.035, 169)]
                                            
        [InlineData(SimulationType.Queuing, 92001, 0.00, 0.035, 169)]
        [InlineData(SimulationType.Queuing, 92011, 0.10, 0.035, 169)]
        [InlineData(SimulationType.Queuing, 92021, 0.15, 0.035, 169)]
        [InlineData(SimulationType.Queuing, 92031, 0.20, 0.035, 169)]
        [InlineData(SimulationType.Queuing, 92041, 0.25, 0.035, 169)]
        [InlineData(SimulationType.Queuing, 92051, 0.30, 0.035, 169)]
        [InlineData(SimulationType.Queuing, 92061, 0.35, 0.035, 169)]
                                            
        [InlineData(SimulationType.Queuing, 92002, 0.00, 0.035, 169)]
        [InlineData(SimulationType.Queuing, 92012, 0.10, 0.035, 169)]
        [InlineData(SimulationType.Queuing, 92022, 0.15, 0.035, 169)]
        [InlineData(SimulationType.Queuing, 92032, 0.20, 0.035, 169)]
        [InlineData(SimulationType.Queuing, 92042, 0.25, 0.035, 169)]
        [InlineData(SimulationType.Queuing, 92052, 0.30, 0.035, 169)]
        [InlineData(SimulationType.Queuing, 92062, 0.35, 0.035, 169)]
                                            
        [InlineData(SimulationType.Queuing, 92003, 0.00, 0.035, 169)]
        [InlineData(SimulationType.Queuing, 92013, 0.10, 0.035, 169)]
        [InlineData(SimulationType.Queuing, 92023, 0.15, 0.035, 169)]
        [InlineData(SimulationType.Queuing, 92033, 0.20, 0.035, 169)]
        [InlineData(SimulationType.Queuing, 92043, 0.25, 0.035, 169)]
        [InlineData(SimulationType.Queuing, 92053, 0.30, 0.035, 169)]
        [InlineData(SimulationType.Queuing, 92063, 0.35, 0.035, 169)]
                                            
        [InlineData(SimulationType.Queuing, 92004, 0.00, 0.035, 169)]
        [InlineData(SimulationType.Queuing, 92014, 0.10, 0.035, 169)]
        [InlineData(SimulationType.Queuing, 92024, 0.15, 0.035, 169)]
        [InlineData(SimulationType.Queuing, 92034, 0.20, 0.035, 169)]
        [InlineData(SimulationType.Queuing, 92044, 0.25, 0.035, 169)]
        [InlineData(SimulationType.Queuing, 92054, 0.30, 0.035, 169)]
        [InlineData(SimulationType.Queuing, 92064, 0.35, 0.035, 169)]
                                            
        [InlineData(SimulationType.Queuing, 92005, 0.00, 0.035, 169)]
        [InlineData(SimulationType.Queuing, 92015, 0.10, 0.035, 169)]
        [InlineData(SimulationType.Queuing, 92025, 0.15, 0.035, 169)]
        [InlineData(SimulationType.Queuing, 92035, 0.20, 0.035, 169)]
        [InlineData(SimulationType.Queuing, 92045, 0.25, 0.035, 169)]
        [InlineData(SimulationType.Queuing, 92055, 0.30, 0.035, 169)]
        [InlineData(SimulationType.Queuing, 92065, 0.35, 0.035, 169)]
                                            
        [InlineData(SimulationType.Queuing, 92006, 0.00, 0.035, 169)]
        [InlineData(SimulationType.Queuing, 92016, 0.10, 0.035, 169)]
        [InlineData(SimulationType.Queuing, 92026, 0.15, 0.035, 169)]
        [InlineData(SimulationType.Queuing, 92036, 0.20, 0.035, 169)]
        [InlineData(SimulationType.Queuing, 92046, 0.25, 0.035, 169)]
        [InlineData(SimulationType.Queuing, 92056, 0.30, 0.035, 169)]
        [InlineData(SimulationType.Queuing, 92066, 0.35, 0.035, 169)]
                                            
        [InlineData(SimulationType.Queuing, 92007, 0.00, 0.035, 169)]
        [InlineData(SimulationType.Queuing, 92017, 0.10, 0.035, 169)]
        [InlineData(SimulationType.Queuing, 92027, 0.15, 0.035, 169)]
        [InlineData(SimulationType.Queuing, 92037, 0.20, 0.035, 169)]
        [InlineData(SimulationType.Queuing, 92047, 0.25, 0.035, 169)]
        [InlineData(SimulationType.Queuing, 92057, 0.30, 0.035, 169)]
        [InlineData(SimulationType.Queuing, 92067, 0.35, 0.035, 169)]
                                            
        [InlineData(SimulationType.Queuing, 92008, 0.00, 0.035, 169)]
        [InlineData(SimulationType.Queuing, 92018, 0.10, 0.035, 169)]
        [InlineData(SimulationType.Queuing, 92028, 0.15, 0.035, 169)]
        [InlineData(SimulationType.Queuing, 92038, 0.20, 0.035, 169)]
        [InlineData(SimulationType.Queuing, 92048, 0.25, 0.035, 169)]
        [InlineData(SimulationType.Queuing, 92058, 0.30, 0.035, 169)]
        [InlineData(SimulationType.Queuing, 92068, 0.35, 0.035, 169)]
                                            
        [InlineData(SimulationType.Queuing, 92009, 0.00, 0.035, 169)]
        [InlineData(SimulationType.Queuing, 92019, 0.10, 0.035, 169)]
        [InlineData(SimulationType.Queuing, 92029, 0.15, 0.035, 169)]
        [InlineData(SimulationType.Queuing, 92039, 0.20, 0.035, 169)]
        [InlineData(SimulationType.Queuing, 92049, 0.25, 0.035, 169)]
        [InlineData(SimulationType.Queuing, 92059, 0.30, 0.035, 169)]
        [InlineData(SimulationType.Queuing, 92069, 0.35, 0.035, 169)]
                                            
        [InlineData(SimulationType.Central, 93000, 0.00, 0.035, 169)]
        [InlineData(SimulationType.Central, 93010, 0.10, 0.035, 169)]
        [InlineData(SimulationType.Central, 93020, 0.15, 0.035, 169)]
        [InlineData(SimulationType.Central, 93030, 0.20, 0.035, 169)]
        [InlineData(SimulationType.Central, 93040, 0.25, 0.035, 169)]
        [InlineData(SimulationType.Central, 93050, 0.30, 0.035, 169)]
        [InlineData(SimulationType.Central, 93060, 0.35, 0.035, 169)]
                                            
        [InlineData(SimulationType.Central, 93001, 0.00, 0.035, 169)]
        [InlineData(SimulationType.Central, 93011, 0.10, 0.035, 169)]
        [InlineData(SimulationType.Central, 93021, 0.15, 0.035, 169)]
        [InlineData(SimulationType.Central, 93031, 0.20, 0.035, 169)]
        [InlineData(SimulationType.Central, 93041, 0.25, 0.035, 169)]
        [InlineData(SimulationType.Central, 93051, 0.30, 0.035, 169)]
        [InlineData(SimulationType.Central, 93061, 0.35, 0.035, 169)]
                                            
        [InlineData(SimulationType.Central, 93002, 0.00, 0.035, 169)]
        [InlineData(SimulationType.Central, 93012, 0.10, 0.035, 169)]
        [InlineData(SimulationType.Central, 93022, 0.15, 0.035, 169)]
        [InlineData(SimulationType.Central, 93032, 0.20, 0.035, 169)]
        [InlineData(SimulationType.Central, 93042, 0.25, 0.035, 169)]
        [InlineData(SimulationType.Central, 93052, 0.30, 0.035, 169)]
        [InlineData(SimulationType.Central, 93062, 0.35, 0.035, 169)]
                                            
        [InlineData(SimulationType.Central, 93003, 0.00, 0.035, 169)]
        [InlineData(SimulationType.Central, 93013, 0.10, 0.035, 169)]
        [InlineData(SimulationType.Central, 93023, 0.15, 0.035, 169)]
        [InlineData(SimulationType.Central, 93033, 0.20, 0.035, 169)]
        [InlineData(SimulationType.Central, 93043, 0.25, 0.035, 169)]
        [InlineData(SimulationType.Central, 93053, 0.30, 0.035, 169)]
        [InlineData(SimulationType.Central, 93063, 0.35, 0.035, 169)]
                                            
        [InlineData(SimulationType.Central, 93004, 0.00, 0.035, 169)]
        [InlineData(SimulationType.Central, 93014, 0.10, 0.035, 169)]
        [InlineData(SimulationType.Central, 93024, 0.15, 0.035, 169)]
        [InlineData(SimulationType.Central, 93034, 0.20, 0.035, 169)]
        [InlineData(SimulationType.Central, 93044, 0.25, 0.035, 169)]
        [InlineData(SimulationType.Central, 93054, 0.30, 0.035, 169)]
        [InlineData(SimulationType.Central, 93064, 0.35, 0.035, 169)]
                                            
        [InlineData(SimulationType.Central, 93005, 0.00, 0.035, 169)]
        [InlineData(SimulationType.Central, 93015, 0.10, 0.035, 169)]
        [InlineData(SimulationType.Central, 93025, 0.15, 0.035, 169)]
        [InlineData(SimulationType.Central, 93035, 0.20, 0.035, 169)]
        [InlineData(SimulationType.Central, 93045, 0.25, 0.035, 169)]
        [InlineData(SimulationType.Central, 93055, 0.30, 0.035, 169)]
        [InlineData(SimulationType.Central, 93065, 0.35, 0.035, 169)]
                                            
        [InlineData(SimulationType.Central, 93006, 0.00, 0.035, 169)]
        [InlineData(SimulationType.Central, 93016, 0.10, 0.035, 169)]
        [InlineData(SimulationType.Central, 93026, 0.15, 0.035, 169)]
        [InlineData(SimulationType.Central, 93036, 0.20, 0.035, 169)]
        [InlineData(SimulationType.Central, 93046, 0.25, 0.035, 169)]
        [InlineData(SimulationType.Central, 93056, 0.30, 0.035, 169)]
        [InlineData(SimulationType.Central, 93066, 0.35, 0.035, 169)]
                                            
        [InlineData(SimulationType.Central, 93007, 0.00, 0.035, 169)]
        [InlineData(SimulationType.Central, 93017, 0.10, 0.035, 169)]
        [InlineData(SimulationType.Central, 93027, 0.15, 0.035, 169)]
        [InlineData(SimulationType.Central, 93037, 0.20, 0.035, 169)]
        [InlineData(SimulationType.Central, 93047, 0.25, 0.035, 169)]
        [InlineData(SimulationType.Central, 93057, 0.30, 0.035, 169)]
        [InlineData(SimulationType.Central, 93067, 0.35, 0.035, 169)]
                                            
        [InlineData(SimulationType.Central, 93008, 0.00, 0.035, 169)]
        [InlineData(SimulationType.Central, 93018, 0.10, 0.035, 169)]
        [InlineData(SimulationType.Central, 93028, 0.15, 0.035, 169)]
        [InlineData(SimulationType.Central, 93038, 0.20, 0.035, 169)]
        [InlineData(SimulationType.Central, 93048, 0.25, 0.035, 169)]
        [InlineData(SimulationType.Central, 93058, 0.30, 0.035, 169)]
        [InlineData(SimulationType.Central, 93068, 0.35, 0.035, 169)]
                                            
        [InlineData(SimulationType.Central, 93009, 0.00, 0.035, 169)]
        [InlineData(SimulationType.Central, 93019, 0.10, 0.035, 169)]
        [InlineData(SimulationType.Central, 93029, 0.15, 0.035, 169)]
        [InlineData(SimulationType.Central, 93039, 0.20, 0.035, 169)]
        [InlineData(SimulationType.Central, 93049, 0.25, 0.035, 169)]
        [InlineData(SimulationType.Central, 93059, 0.30, 0.035, 169)]
        [InlineData(SimulationType.Central, 93069, 0.35, 0.035, 169)]

        [InlineData(SimulationType.Default, 10010, 0.20, 0.035, 169, 5, 5, 2, 0.0)]
        [InlineData(SimulationType.Default, 10011, 0.20, 0.035, 169, 5, 5, 2, 0.1)]
        [InlineData(SimulationType.Default, 10012, 0.20, 0.035, 169, 5, 5, 2, 0.2)]
        [InlineData(SimulationType.Default, 10013, 0.20, 0.035, 169, 5, 5, 2, 0.3)]
        [InlineData(SimulationType.Default, 10014, 0.20, 0.035, 169, 5, 5, 2, 0.4)]
        [InlineData(SimulationType.Default, 10015, 0.20, 0.035, 169, 5, 5, 2, 0.5)]
        [InlineData(SimulationType.Default, 10016, 0.20, 0.035, 169, 5, 5, 2, 0.6)]
        [InlineData(SimulationType.Default, 10017, 0.20, 0.035, 169, 5, 5, 2, 0.7)]
        [InlineData(SimulationType.Default, 10018, 0.20, 0.035, 169, 5, 5, 2, 0.8)]
        [InlineData(SimulationType.Default, 10019, 0.20, 0.035, 169, 5, 5, 2, 0.9)]
        [InlineData(SimulationType.Default, 10020, 0.20, 0.035, 169, 5, 5, 2, 1)]

        [InlineData(SimulationType.Default, 10110, 0.20, 0.035, 169, 5, 1, 1.41, 0.0)]
        [InlineData(SimulationType.Default, 10111, 0.20, 0.035, 169, 5, 1, 1.41, 0.1)]
        [InlineData(SimulationType.Default, 10112, 0.20, 0.035, 169, 5, 1, 1.41, 0.2)]
        [InlineData(SimulationType.Default, 10113, 0.20, 0.035, 169, 5, 1, 1.41, 0.3)]
        [InlineData(SimulationType.Default, 10114, 0.20, 0.035, 169, 5, 1, 1.41, 0.4)]
        [InlineData(SimulationType.Default, 10115, 0.20, 0.035, 169, 5, 1, 1.41, 0.5)]
        [InlineData(SimulationType.Default, 10116, 0.20, 0.035, 169, 5, 1, 1.41, 0.6)]
        [InlineData(SimulationType.Default, 10117, 0.20, 0.035, 169, 5, 1, 1.41, 0.7)]
        [InlineData(SimulationType.Default, 10118, 0.20, 0.035, 169, 5, 1, 1.41, 0.8)]
        [InlineData(SimulationType.Default, 10119, 0.20, 0.035, 169, 5, 1, 1.41, 0.9)]
        [InlineData(SimulationType.Default, 10120, 0.20, 0.035, 169, 5, 1, 1.41, 1)]

        [InlineData(SimulationType.Queuing, 20010, 0.20, 0.035, 169, 5, 5, 2, 0.0)]
        [InlineData(SimulationType.Queuing, 20011, 0.20, 0.035, 169, 5, 5, 2, 0.1)]
        [InlineData(SimulationType.Queuing, 20012, 0.20, 0.035, 169, 5, 5, 2, 0.2)]
        [InlineData(SimulationType.Queuing, 20013, 0.20, 0.035, 169, 5, 5, 2, 0.3)]
        [InlineData(SimulationType.Queuing, 20014, 0.20, 0.035, 169, 5, 5, 2, 0.4)]
        [InlineData(SimulationType.Queuing, 20015, 0.20, 0.035, 169, 5, 5, 2, 0.5)]
        [InlineData(SimulationType.Queuing, 20016, 0.20, 0.035, 169, 5, 5, 2, 0.6)]
        [InlineData(SimulationType.Queuing, 20017, 0.20, 0.035, 169, 5, 5, 2, 0.7)]
        [InlineData(SimulationType.Queuing, 20018, 0.20, 0.035, 169, 5, 5, 2, 0.8)]
        [InlineData(SimulationType.Queuing, 20019, 0.20, 0.035, 169, 5, 5, 2, 0.9)]
        [InlineData(SimulationType.Queuing, 20020, 0.20, 0.035, 169, 5, 5, 2, 1)]
                                            
        [InlineData(SimulationType.Queuing, 20110, 0.20, 0.035, 169, 5, 1, 1.41, 0.0)]
        [InlineData(SimulationType.Queuing, 20111, 0.20, 0.035, 169, 5, 1, 1.41, 0.1)]
        [InlineData(SimulationType.Queuing, 20112, 0.20, 0.035, 169, 5, 1, 1.41, 0.2)]
        [InlineData(SimulationType.Queuing, 20113, 0.20, 0.035, 169, 5, 1, 1.41, 0.3)]
        [InlineData(SimulationType.Queuing, 20114, 0.20, 0.035, 169, 5, 1, 1.41, 0.4)]
        [InlineData(SimulationType.Queuing, 20115, 0.20, 0.035, 169, 5, 1, 1.41, 0.5)]
        [InlineData(SimulationType.Queuing, 20116, 0.20, 0.035, 169, 5, 1, 1.41, 0.6)]
        [InlineData(SimulationType.Queuing, 20117, 0.20, 0.035, 169, 5, 1, 1.41, 0.7)]
        [InlineData(SimulationType.Queuing, 20118, 0.20, 0.035, 169, 5, 1, 1.41, 0.8)]
        [InlineData(SimulationType.Queuing, 20119, 0.20, 0.035, 169, 5, 1, 1.41, 0.9)]
        [InlineData(SimulationType.Queuing, 20120, 0.20, 0.035, 169, 5, 1, 1.41, 1)]

        [InlineData(SimulationType.Central, 30010, 0.20, 0.035, 169, 5, 5, 2, 0.0)]
        [InlineData(SimulationType.Central, 30011, 0.20, 0.035, 169, 5, 5, 2, 0.1)]
        [InlineData(SimulationType.Central, 30012, 0.20, 0.035, 169, 5, 5, 2, 0.2)]
        [InlineData(SimulationType.Central, 30013, 0.20, 0.035, 169, 5, 5, 2, 0.3)]
        [InlineData(SimulationType.Central, 30014, 0.20, 0.035, 169, 5, 5, 2, 0.4)]
        [InlineData(SimulationType.Central, 30015, 0.20, 0.035, 169, 5, 5, 2, 0.5)]
        [InlineData(SimulationType.Central, 30016, 0.20, 0.035, 169, 5, 5, 2, 0.6)]
        [InlineData(SimulationType.Central, 30017, 0.20, 0.035, 169, 5, 5, 2, 0.7)]
        [InlineData(SimulationType.Central, 30018, 0.20, 0.035, 169, 5, 5, 2, 0.8)]
        [InlineData(SimulationType.Central, 30019, 0.20, 0.035, 169, 5, 5, 2, 0.9)]
        [InlineData(SimulationType.Central, 30020, 0.20, 0.035, 169, 5, 5, 2, 1)]
                                            
        [InlineData(SimulationType.Central, 30110, 0.20, 0.035, 169, 5, 1, 1.41, 0.0)]
        [InlineData(SimulationType.Central, 30111, 0.20, 0.035, 169, 5, 1, 1.41, 0.1)]
        [InlineData(SimulationType.Central, 30112, 0.20, 0.035, 169, 5, 1, 1.41, 0.2)]
        [InlineData(SimulationType.Central, 30113, 0.20, 0.035, 169, 5, 1, 1.41, 0.3)]
        [InlineData(SimulationType.Central, 30114, 0.20, 0.035, 169, 5, 1, 1.41, 0.4)]
        [InlineData(SimulationType.Central, 30115, 0.20, 0.035, 169, 5, 1, 1.41, 0.5)]
        [InlineData(SimulationType.Central, 30116, 0.20, 0.035, 169, 5, 1, 1.41, 0.6)]
        [InlineData(SimulationType.Central, 30117, 0.20, 0.035, 169, 5, 1, 1.41, 0.7)]
        [InlineData(SimulationType.Central, 30118, 0.20, 0.035, 169, 5, 1, 1.41, 0.8)]
        [InlineData(SimulationType.Central, 30119, 0.20, 0.035, 169, 5, 1, 1.41, 0.9)]
        [InlineData(SimulationType.Central, 30120, 0.20, 0.035, 169, 5, 1, 1.41, 1)]

        public async Task AgentSystemTest(SimulationType simulationType, int simNr, double deviation, double arrivalRateRun, int seed
            , int seedDataGen = 5, double reuse = 1.3, double complxity = 1.9, double organziationaldegree = 0.7)
        {
            //var simNr = Random.Shared.Next();
            //var simulationType = SimulationType.Default;
            
            var throughput = 1440 * 5;
            var arrivalRate = arrivalRateRun;

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

            SeedInitializer seedInitializer = new SeedInitializer();
            seedInitializer.GenerateTestData(TestMateDb, machineCount: 4, toolCount: 6
                                             , seed: seedDataGen
                                             , reuseRatio: reuse
                                             , complexityRatio: complxity
                                             , organizationalDegree: organziationaldegree);
            
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
            simConfig.ReplaceOption(new TimeConstraintQueueLength(480 * 6 * 2)); // = schicht * setups * x
            simConfig.ReplaceOption(new SimulationKind(value: simulationType));
            simConfig.ReplaceOption(new OrderArrivalRate(value: arrivalRate));
            simConfig.ReplaceOption(new OrderQuantity(value: 10000));
            simConfig.ReplaceOption(new EstimatedThroughPut(value: throughput));
            simConfig.ReplaceOption(new TimePeriodForThroughputCalculation(value: 4000));
            simConfig.ReplaceOption(new Production.Core.Environment.Options.Seed(value: seed));
            simConfig.ReplaceOption(new SettlingStart(value: 2880));
            simConfig.ReplaceOption(new MinDeliveryTime(value: 10));
            simConfig.ReplaceOption(new MaxDeliveryTime(value: 15));
            simConfig.ReplaceOption(new SimulationEnd(value: 10080 * 3));
            simConfig.ReplaceOption(new SaveToDB(value: true));
            simConfig.ReplaceOption(new DebugSystem(value: true));
            simConfig.ReplaceOption(new DebugAgents(value: true));
            simConfig.ReplaceOption(new WorkTimeDeviation(deviation));
            simConfig.ReplaceOption(new MaxBucketSize(value: 480 * 6 * 2)); // = schicht * setups * x
            simConfig.ReplaceOption(new SimulationNumber(value: simNr));
            simConfig.ReplaceOption(new CreateQualityData(false));
            simConfig.ReplaceOption(new Mate.Production.Core.Environment.Options.PriorityRule(PriorityRule.LST));

            ClearResultDBby(simNr: simConfig.GetOption<SimulationNumber>(), dbName: TestMateResultDb);

            BaseSimulation simContext;

            if (simulationType == SimulationType.Central)
            {
                var ganttPlanContext = Dbms.GetGanttDataBase(DataBaseConfiguration.GP);
                ganttPlanContext.DbContext.Database.ExecuteSqlRaw("EXEC sp_MSforeachtable 'DELETE FROM ? '");

                //Synchronisation GanttPlan
                GanttPlanOptRunner.RunOptAndExport("Init", "D:\\Work\\GANTTPLAN\\GanttPlanOptRunner.exe");

                simContext = new GanttSimulation(dbName: TestMateDb, messageHub: new ConsoleHub());
            }
            else
            {
                simContext = new AgentSimulation(TestMateDb, messageHub: new ConsoleHub());
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
