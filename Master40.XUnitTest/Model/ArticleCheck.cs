using System.Linq;
using Master40.DB.Data.Context;
using Master40.DB.Data.Initializer;
using Master40.XUnitTest.Preparations;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Master40.XUnitTest.Model
{
    public class ArticleCheck
    {
        private ProductionDomainContext _masterDBContext;
        private string _dbConnectionString;
        public ArticleCheck()
        {
            _dbConnectionString = Dbms.getDbContextString().Replace("UnitTestDB", "TruckTest");
            _masterDBContext = new ProductionDomainContext(options: new DbContextOptionsBuilder<MasterDBContext>()
                .UseSqlServer(connectionString: _dbConnectionString)
                .Options);
            _masterDBContext.Database.EnsureCreated();
            MasterDBInitializerTruck.DbInitialize(context: _masterDBContext);
        }

        [Fact]
        public void HasBoms()
        {
            var articles = _masterDBContext.Articles.Include(x => x.ArticleBoms);
            Assert.True(articles.All(x => x.ArticleBoms.Count >= 0));
        }

        [Fact]
        public void AllBomsWithOperation()
        {
            var boms = _masterDBContext.ArticleBoms;
            Assert.True(boms.All(x => x.OperationId != null));
        }


    }
}
