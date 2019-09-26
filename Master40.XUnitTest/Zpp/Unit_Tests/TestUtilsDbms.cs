using Master40.DB;
using Master40.DB.Data.Context;
using Master40.DB.Data.Helper;
using Microsoft.EntityFrameworkCore;
using Xunit;
using Zpp.Utils;

namespace Master40.XUnitTest.Zpp.Unit_Tests
{
    public class TestUtilsDbms
    {

        [Fact(Skip = "Doesnt work with LocalDB and Unix Systems")]
        public void TestDropExistingDatabase()
        {
            DataBase<ProductionDomainContext> productionDataBase = Dbms.GetNewDataBase();
            if (productionDataBase.DbContext.Database.CanConnect() == false)
            {
                productionDataBase.DbContext.Database.EnsureCreated();
            }

            productionDataBase.DbContext.Database.CloseConnection();

            bool wasDropped = Dbms.DropDatabase(productionDataBase.DataBaseName.Value,
                                                productionDataBase.ConnectionString.Value );
            Assert.True(wasDropped, "Db could not be dropped.");
            Assert.False(productionDataBase.DbContext.Database.CanConnect(),
                "Can still connect to database.");
        }
    }
}