using System.Linq;
using Master40.DB;
using Master40.DB.Data.Context;
using Master40.DB.Data.Helper;
using Master40.DB.Data.Initializer;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Master40.XUnitTest.Online.Model
{
    public class ArticleCheck
    {
        private DataBase<ProductionDomainContext> DataBase;
        public ArticleCheck()
        {
            DataBase = Dbms.GetNewMasterDataBase();
            MasterDBInitializerTruck.DbInitialize(context: DataBase.DbContext, resourceModelSize: ModelSize.Medium,
                setupModelSize: ModelSize.Medium, ModelSize.Small, 3, false);
        }

        [Fact]
        public void HasBoms()
        {
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
