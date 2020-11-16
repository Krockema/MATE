using Akka.TestKit.Xunit;
using Master40.DB;
using Master40.DB.Data.Context;
using Master40.DB.Data.Helper;
using Master40.DB.Data.Initializer;
using Master40.DB.Nominal.Model;
using Xunit;

namespace Master40.XUnitTest.Online.Agents.Types
{
    public class ArticleCache : TestKit
    {
        private DataBase<ProductionDomainContext> _contextDataBase;
        public ArticleCache()
        {

            _contextDataBase = Dbms.GetNewMasterDataBase();

            InitializeTestModel();
        }

        [Fact(Skip = "BUG need to change system for new IDs not given from Db -see new Database for each test")]
        public void AddArticle()
        {
            var _articleCache = new SimulationCore.Types.ArticleCache(connectionString: _contextDataBase.ConnectionString);

            var article = _articleCache.GetArticleById(id: 10081, transitionFactor: 3);
            Assert.Equal(actual: article.Name, expected: "Dump-Truck");

        }
        
        [Fact(Skip = "BUG need to change system for new IDs not given from Db -see new Database for each test")]
        public void AddArticleWithoutOperation()
        {
            _contextDataBase = Dbms.GetNewMasterDataBase();

            InitializeTestModel();
            var _articleCache = new SimulationCore.Types.ArticleCache(connectionString: _contextDataBase.ConnectionString);
            var article = _articleCache.GetArticleById(id: 10456, transitionFactor: 3);
            Assert.Equal(actual: article.Name, expected: "Wheel");

        }

        [Fact(Skip = "BUG need to change system for new IDs not given from Db -see new Database for each test")]
        public void AddExistingArticle()
        {
            _contextDataBase = Dbms.GetNewMasterDataBase();

            var _articleCache = new SimulationCore.Types.ArticleCache(connectionString: _contextDataBase.ConnectionString);
            var article = _articleCache.GetArticleById(id: 10772, transitionFactor: 3);
            var article2 = _articleCache.GetArticleById(id: 10772, transitionFactor: 3);

            Assert.Equal(expected: article2.Name, actual: article.Name);
        }

        private void InitializeTestModel()
        {
            MasterDBInitializerTruck.DbInitialize(_contextDataBase.DbContext, ModelSize.TestModel, ModelSize.Small, ModelSize.Small, 0, false, false);
        }
    }
}
