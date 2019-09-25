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
            ProductionDomainContext productionDomainContext = Dbms.GetDbContext();
            if (productionDomainContext.Database.CanConnect() == false)
            {
                productionDomainContext.Database.EnsureCreated();
            }

            productionDomainContext.Database.CloseConnection();

            bool wasDropped = Dbms.DropDatabase(Constants.GetDbName(), Dbms.GetConnectionString());
            Assert.True(wasDropped, "Db could not be dropped.");
            Assert.False(productionDomainContext.Database.CanConnect(),
                "Can still connect to database.");
        }
    }
}