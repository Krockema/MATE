using System.Collections.Generic;
using Akka.Actor;
using Master40.DB.Data.Context;
using Master40.DB.Data.Initializer;
using Master40.DB.DataModel;
using Master40.SimulationCore.Agents.ProductionAgent.Types;
using Master40.XUnitTest.Preparations;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Master40.XUnitTest.Agents.Types
{
    public class OperationManagers
    {
        private ProductionDomainContext dbContext;
        public OperationManagers()
        {
            dbContext = new ProductionDomainContext(options: new DbContextOptionsBuilder<MasterDBContext>()
                .UseSqlServer(connectionString: Dbms.getDbContextString())
                .Options);
            dbContext.Database.EnsureCreated();
            MasterDBInitializerSimple.DbInitialize(context: dbContext);
        }

        [Fact]
        public void AddAndRetrieveOperationByKey()
        {
            var operationManager = new OperationManager();
            var job = TypeFactory.CreateJobItem(jobName: "Sample Operation 1", jobDuration: 10);
            operationManager.AddOperation(job);
            var returnedJob = operationManager.GetByOperationKey(job.Key);

            Assert.True(returnedJob != null);
        }

        [Fact]

        public void GetOperationsBySkill()
        {
            var operationManager = new OperationManager();
            var job = TypeFactory.CreateJobItem(jobName: "Sample Operation 1", jobDuration: 10);
            var returnedJob = operationManager.GetOperationBySkill("Sewing");
            Assert.True(returnedJob != null);
        }

        [Fact]
        public void SetArticleProvided()
        {
            var operationManager = new OperationManager();
            var job = TypeFactory.CreateJobItem(jobName: "Sample Operation 1", jobDuration: 10);
            var article = new M_Article() { Name = "Bear", ArticleBoms = new List<M_ArticleBom>() { new M_ArticleBom() { ArticleChild = new M_Article() { Name = "Wood"}}}};
            var fArticle = TypeFactory.CreateDummyArticle(59, 0, article, 1);
            operationManager.AddOperation(job);
            var numberOfRequiredDispos = operationManager.CreateRequiredArticles(fArticle, ActorRefs.Nobody, 0);
            Assert.True(numberOfRequiredDispos == 1);
        }
        [Fact]
        public void UpdateOperations() { }
        [Fact]
        public void ProvideArticle()
        {
            var operationManager = new OperationManager();
            var job = TypeFactory.CreateJobItem(jobName: "Sample Operation 1", jobDuration: 10);
            operationManager.AddOperation(job);
            var returnedJob = operationManager.GetByOperationKey(job.Key);

            Assert.True(returnedJob != null);
        }
    }
}