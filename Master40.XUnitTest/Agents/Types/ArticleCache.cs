using Master40.DB.DataModel;
using System;
using System.Collections.Generic;
using Master40.SimulationCore.Types;
using System.Text;
using Xunit;
using static FArticles;
using Akka.TestKit.Xunit;
using Akka.Actor;
using Master40.DB.Data.Context;
using Microsoft.EntityFrameworkCore;
using Master40.DB.Data.Initializer;

namespace Master40.XUnitTest.Agents.Types
{
    public class ArticleCache : TestKit
    {
        static ProductionDomainContext _ctx = new ProductionDomainContext(new DbContextOptionsBuilder<MasterDBContext>()
                                                    .UseInMemoryDatabase(databaseName: "InMemoryDB")
                                                    .Options);

        SimulationCore.Types.ArticleCache _articleCache = new SimulationCore.Types.ArticleCache(_ctx.Database.GetDbConnection().ConnectionString);

        public ArticleCache()
        {
            _ctx.Database.EnsureCreated();
            MasterDBInitializerSimple.DbInitialize(_ctx);
         }


        [Fact]
        public void AddArticle()
        {
            var storageAgentRef = CreateTestProbe();
            var originAgentRef = CreateTestProbe();
            var dispoAgentRef = CreateTestProbe();
            List<IActorRef> providerList = new List<IActorRef>();
            providerList.Add(CreateTestProbe());
            providerList.Add(CreateTestProbe());

            //FArticle fArticle = new FArticle(Guid.NewGuid(), 0, _ctx.Articles. Guid.NewGuid(), storageAgentRef, 2, 30, originAgentRef, dispoAgentRef, providerList, 1, false, false, 0L); ;

            //_articleCache.GetArticleById(fArticle.Article.Id);


            Assert.Equal(0,0);
        }

        [Fact]
        public void AddExistingArticle()
        {
            throw new Exception("Not Testet Yet.");
        }

        [Fact]
        public void GetArticle()
        {
            throw new Exception("Not Testet Yet.");
        }

    }
}
