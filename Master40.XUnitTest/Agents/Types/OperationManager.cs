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
            operationManager.AddOperation(job);
            operationManager.CreateRequiredArticles(new M_Article { Name = "Bear" })
            Assert.True(returnedJob != null);
        }
        [Fact]
        public void UpdateOperations() { }
        [Fact]
        public void ProvideArticle()
        {
            OperationManager.
            var job = TypeFactory.CreateJobItem(jobName: "Sample Operation 1", jobDuration: 10);
            OperationManager.AddOperation(job);
            var returnedJob = OperationManager.GetByOperationKey(job.Key);

            Assert.True(returnedJob != null);
        }
    }
}