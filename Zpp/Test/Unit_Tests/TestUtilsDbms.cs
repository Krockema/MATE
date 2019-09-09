using Master40.DB.Data.Context;
using Microsoft.EntityFrameworkCore;
using Xunit;
using Zpp.Utils;

namespace Zpp.Test.Unit_Tests
{
    public class TestUtilsDbms
    {

        [Fact(Skip = "Sql server 'drop database' does not work on non-Windows-systems.")]
        public void TestDropExistingDatabase()
        {
            ProductionDomainContext productionDomainContext = Dbms.GetDbContext();
            if (productionDomainContext.Database.CanConnect() == false)
            {
                productionDomainContext.Database.EnsureCreated();
            }

            productionDomainContext.Database.CloseConnection();

            bool wasDropped = Dbms.DropDatabase(Constants.GetDbName());
            Assert.True(wasDropped, "Db could not be dropped.");
            Assert.False(productionDomainContext.Database.CanConnect(),
                "Can still connect to database.");
        }
        
        public void TestDropNonExistingDatabase()
        {
            bool wasDropped = Dbms.DropDatabase("bla");
            Assert.False(wasDropped, "Db could be dropped, although it doesn't exist.");
        }
    }
}