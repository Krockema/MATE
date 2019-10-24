﻿using Akka.TestKit.Xunit;
using Master40.DB.Data.Context;
using Master40.DB.Data.Initializer;
using Master40.XUnitTest.Preparations;
using Microsoft.EntityFrameworkCore;
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
            _masterDBContext = new ProductionDomainContext(options: new DbContextOptionsBuilder<MasterDBContext>()
                                .UseSqlServer(connectionString: _dbConnectionString)
                                .Options);
            _masterDBContext.Database.EnsureDeleted();
            _masterDBContext.Database.EnsureCreated();
            MasterDbInitializerTable.DbInitialize(context: _masterDBContext);
         }


        [Fact(Skip = "Datamodel fix required")]
        public void AddArticle()
        {
            var _articleCache = new SimulationCore.Types.ArticleCache(connectionString: _dbConnectionString);
            //BUG need to change system for new IDs not given from Db - see new Database for each test
            var article = _articleCache.GetArticleById(id: 1, transitionFactor: 3);
            Assert.Equal(actual: article.Name, expected: "Tisch");
        }

        [Fact(Skip = "Datamodel fix required")]
        public void AddArticleWithoutOperation()
        {
            var _articleCache = new SimulationCore.Types.ArticleCache(connectionString: _dbConnectionString);
            var article = _articleCache.GetArticleById(id: 6, transitionFactor: 3);
            Assert.Equal(actual: article.Name, expected: "Schrauben");
        }

        [Fact(Skip = "Datamodel fix required")]
        public void AddExistingArticle()
        {
            var _articleCache = new SimulationCore.Types.ArticleCache(connectionString: _dbConnectionString);
            var article = _articleCache.GetArticleById(id: 6, transitionFactor: 3);
            var article2 = _articleCache.GetArticleById(id: 6, transitionFactor: 3);

            Assert.Equal(expected: article2.Name, actual: article.Name);
        }
    }
}
