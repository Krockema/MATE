using Master40.DB;
using Master40.DB.Data.Context;
using Master40.DB.Data.Helper;
using Master40.DB.Data.Initializer;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using Xunit;

namespace Master40.XUnitTest.Model
{
    public class ArticleCheck
    {
        private DataBase<ProductionDomainContext> DataBase;
        public ArticleCheck()
        {
            DataBase = Dbms.GetNewDataBase();
            MasterDBInitializerTruck.DbInitialize(context: DataBase.DbContext);
        }

        [Fact(Skip = "Activate after merge")]
        public void HasBoms()
        {
            var articles = DataBase.DbContext.Articles.Include(x => x.ArticleBoms);
            Assert.True(articles.All(x => x.ArticleBoms.Count >= 0));
        }

        [Fact(Skip = "Activate after merge")]
        public void AllBomsWithOperation()
        {
            var boms = DataBase.DbContext.ArticleBoms;
            Assert.True(boms.All(x => x.OperationId != null));
        }


    }
}
