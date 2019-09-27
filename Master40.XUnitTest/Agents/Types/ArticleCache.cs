using System.Linq;
using Akka.TestKit.Xunit;
using Master40.DB;
using Master40.DB.Data.Context;
using Master40.DB.Data.Helper;
using Master40.DB.Data.Initializer;
using Master40.XUnitTest.Preparations;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Master40.XUnitTest.Agents.Types
{
    public class ArticleCache : TestKit
    {
        private DataBase<ProductionDomainContext> DataBase;
        public ArticleCache()
        {
            DataBase = Dbms.GetNewMasterDataBase();
            DataBase.DbContext.Database.EnsureCreated();
            MasterDbInitializerTable.DbInitialize(context: DataBase.DbContext);
        }



        [Fact]
        public void AddArticle()
        {
            var _articleCache = new SimulationCore.Types.ArticleCache(connectionString: DataBase.ConnectionString.Value);
            var requestArticle = DataBase.DbContext.Articles.First();
            var article = _articleCache.GetArticleById(id: requestArticle.Id, transitionFactor: 3);
            Assert.Equal(actual: article.Name, expected: "Tisch");
        }

        [Fact]
        public void AddArticleWithoutOperation()
        {
            var _articleCache = new SimulationCore.Types.ArticleCache(connectionString: DataBase.ConnectionString.Value);
            var requestArticle = DataBase.DbContext.Articles.First(x => x.Name == "Schrauben");
            var article = _articleCache.GetArticleById(id: requestArticle.Id, transitionFactor: 3);
            Assert.Equal(actual: article.Name, expected: "Schrauben");
        }

        [Fact]
        public void AddExistingArticle()
        {
            var _articleCache = new SimulationCore.Types.ArticleCache(connectionString: DataBase.ConnectionString.Value);
            var requestArticle = DataBase.DbContext.Articles.First(x => x.Name == "Schrauben");
            var article = _articleCache.GetArticleById(id: requestArticle.Id, transitionFactor: 3);
            var article2 = _articleCache.GetArticleById(id: requestArticle.Id, transitionFactor: 3);

            Assert.Equal(expected: article2.Name, actual: article.Name);
        }
    }
}
