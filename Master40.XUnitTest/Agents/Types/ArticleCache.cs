using Akka.TestKit.Xunit;
using Master40.DB.Data.Context;
using Master40.DB.Data.Initializer;
using Master40.XUnitTest.Preparations;
using Microsoft.EntityFrameworkCore;
using System;
using Xunit;

namespace Master40.XUnitTest.Agents.Types
{
    public class ArticleCache : TestKit
    {
        private ProductionDomainContext _masterDBContext;
        private string _dbConnectionString;
        public ArticleCache()
        {
            _dbConnectionString = Dbms.getDbContextString();
            _masterDBContext = new ProductionDomainContext(new DbContextOptionsBuilder<MasterDBContext>()
                                .UseSqlServer(_dbConnectionString)
                                .Options);
            _masterDBContext.Database.EnsureDeleted();
            _masterDBContext.Database.EnsureCreated();
            MasterDBInitializerSimple.DbInitialize(_masterDBContext);
         }


        [Fact]
        public void AddArticle()
        {
            var _articleCache = new SimulationCore.Types.ArticleCache(_dbConnectionString);
            var article = _articleCache.GetArticleById(id: 1, transitionFactor: 3);
            Assert.Equal(actual: article.Name, expected: "Tisch");
        }

        [Fact]
        public void AddArticleWithoutOperation()
        {
            var _articleCache = new SimulationCore.Types.ArticleCache(_dbConnectionString);
            var article = _articleCache.GetArticleById(id: 6, transitionFactor: 3);
            Assert.Equal(actual: article.Name, expected: "Schrauben");
        }

        [Fact]
        public void AddExistingArticle()
        {
            var _articleCache = new SimulationCore.Types.ArticleCache(_dbConnectionString);
            var article = _articleCache.GetArticleById(id: 6, transitionFactor: 3);
            var article2 = _articleCache.GetArticleById(id: 6, transitionFactor: 3);

            Assert.Equal(article2.Name, article.Name);
        }
    }
}
