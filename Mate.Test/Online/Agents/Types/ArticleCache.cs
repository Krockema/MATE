using Akka.TestKit.Xunit;
using Mate.DataCore;
using Mate.DataCore.Data.Context;
using Mate.DataCore.Data.Helper;
using Mate.DataCore.Data.Initializer;
using Mate.DataCore.Nominal.Model;
using Xunit;

namespace Mate.Test.Online.Agents.Types
{
    public class ArticleCache : TestKit
    {
        private DataBase<MateProductionDb> _contextDataBase;
        public ArticleCache()
        {

            _contextDataBase = Dbms.GetNewMateDataBase();

            InitializeTestModel();
        }

        [Fact(Skip = "BUG need to change system for new IDs not given from Db -see new Database for each test")]
        public void AddArticle()
        {
            var _articleCache = new Mate.Production.Core.Types.ArticleCache(connectionString: _contextDataBase.ConnectionString);

            var article = _articleCache.GetArticleById(id: 10081, transitionFactor: 3);
            Assert.Equal(actual: article.Name, expected: "Dump-Truck");

        }
        
        [Fact(Skip = "BUG need to change system for new IDs not given from Db -see new Database for each test")]
        public void AddArticleWithoutOperation()
        {
            _contextDataBase = Dbms.GetNewMateDataBase();

            InitializeTestModel();
            var _articleCache = new Mate.Production.Core.Types.ArticleCache(connectionString: _contextDataBase.ConnectionString);
            var article = _articleCache.GetArticleById(id: 10456, transitionFactor: 3);
            Assert.Equal(actual: article.Name, expected: "Wheel");

        }

        [Fact(Skip = "BUG need to change system for new IDs not given from Db -see new Database for each test")]
        public void AddExistingArticle()
        {
            _contextDataBase = Dbms.GetNewMateDataBase();

            var _articleCache = new Mate.Production.Core.Types.ArticleCache(connectionString: _contextDataBase.ConnectionString);
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
