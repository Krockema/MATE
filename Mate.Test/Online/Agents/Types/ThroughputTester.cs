using Mate.DataCore;
using Mate.DataCore.Data.Context;
using Mate.DataCore.Data.Helper;
using Mate.DataCore.Data.Initializer;
using Mate.DataCore.Nominal.Model;
using Mate.Production.Core.Agents.SupervisorAgent.Types;
using Mate.Test.SimulationEnvironment;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Mate.Test.Online.Agents.Types
{
    public class ThroughputTester
    {
        private DataBase<MateProductionDb> _contextDataBase;


        private ThroughputTimeAnalyzer throughputTimeAnalyzer = new();

        private SeedInitializer seedInitializer = new SeedInitializer();

        private readonly string TestMateDb = "Test" + DataBaseConfiguration.MateDb;

        public ThroughputTester()
        {

            _contextDataBase = Dbms.GetNewMateDataBase();

            InitializeTestModel();
        }

        private void InitializeTestModel()
        {
            seedInitializer.GenerateTestData(TestMateDb);
        }

        [Fact]
        public void ThroughputTest()
        {
            var masterCtx = Dbms.GetMateDataBase(dbName: TestMateDb);

            Mate.Production.Core.Types.ArticleCache articleCache = new(new DataCore.Data.Helper.Types.DbConnectionString(masterCtx.ConnectionString.Value));

            var type = masterCtx.DbContext.ArticleTypes.First(x => x.Name == "Product");

            var products = masterCtx.DbContext.Articles.Where(x => x.ArticleTypeId == type.Id);

            var time = throughputTimeAnalyzer.GetTimeForProduct(articleCache, products.First().Id, 0);


            Assert.True(true);
        }

    }
}
