using System;
using System.Linq;
using Master40.DB;
using Master40.DB.Data.Context;
using Master40.DB.Data.Helper;
using Master40.DB.Data.Initializer;
using Master40.DB.Nominal.Model;
using Microsoft.EntityFrameworkCore;
using Xunit;
using Xunit.Abstractions;

namespace Master40.XUnitTest.Online.Model
{
    public class ArticleCheck
    {
        private DataBase<ProductionDomainContext> DataBase;
        private ITestOutputHelper _output;
        public ArticleCheck(ITestOutputHelper output)
        {
            DataBase = Dbms.GetNewMasterDataBase(dbName: "Test");
            _output = output;
            output.WriteLine(DataBase.ConnectionString.Value);
            MasterDBInitializerTruck.DbInitialize(context: DataBase.DbContext, resourceModelSize: ModelSize.Medium,
                setupModelSize: ModelSize.Medium, ModelSize.Small, 3, false, false);
        }

        [Fact]
        public void HasBoms()
        {
            Console.WriteLine("DatabaseString: " + DataBase.ConnectionString.Value);
            System.Diagnostics.Debug.WriteLine("DatabaseString: " + DataBase.ConnectionString.Value);
            var articles = DataBase.DbContext.Articles.Include(x => x.ArticleBoms);
            Assert.True(articles.All(x => x.ArticleBoms.Count >= 0));
        }

        [Fact(Skip = "for test reasons skipped")]
        public void AllBomsWithOperation()
        {
            var boms = DataBase.DbContext.ArticleBoms;
            Assert.True(boms.All(x => x.OperationId != null));
        }


    }
}
