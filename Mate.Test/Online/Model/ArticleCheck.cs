using System;
using System.Data.Entity;
using System.Linq;
using Mate.DataCore;
using Mate.DataCore.Data.Context;
using Mate.DataCore.Data.Helper;
using Mate.DataCore.Data.Initializer;
using Mate.DataCore.Nominal.Model;
using Xunit;
using Xunit.Abstractions;

namespace Mate.Test.Online.Model
{
    public class ArticleCheck
    {
        private DataBase<MateProductionDb> DataBase;
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
