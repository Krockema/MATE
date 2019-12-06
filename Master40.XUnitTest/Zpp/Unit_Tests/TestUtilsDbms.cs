using Master40.DB;
using Master40.DB.Data.Context;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Master40.XUnitTest.Zpp.Unit_Tests
{
    public class TestUtilsDbms
    {
        [Fact(Skip = "Sql server 'drop database' does not work on non-Windows-systems.")]
        public void TestDropExistingDatabase()
        {
            ProductionDomainContext productionDomainContext =Dbms.GetNewMasterDataBase().DbContext;
            
            if (productionDomainContext.Database.CanConnect() == false)
            {
                productionDomainContext.Database.EnsureCreated();
            }

            productionDomainContext.Database.CloseConnection();

            bool wasDropped =
                Dbms.DropDatabase(productionDomainContext.Database.GetDbConnection().Database,
                    productionDomainContext.Database.GetDbConnection().ConnectionString);
            Assert.True(wasDropped, "Db could not be dropped.");
            Assert.False(productionDomainContext.Database.CanConnect(),
                "Can still connect to database.");
        }

        [Fact]
        public void TestDropNonExistingDatabase()
        {
            ProductionDomainContext productionDomainContext =Dbms.GetNewMasterDataBase().DbContext;
            
            bool wasDropped = Dbms.DropDatabase("bla",
                productionDomainContext.Database.GetDbConnection().ConnectionString);
            Assert.False(wasDropped, "Db could be dropped, although it doesn't exist.");
        }
    }
}