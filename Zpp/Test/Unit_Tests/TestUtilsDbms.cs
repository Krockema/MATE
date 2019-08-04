using System.Threading;
using Master40.DB.Data.Context;
using Microsoft.EntityFrameworkCore;
using Xunit;
using Zpp.Utils;

namespace Zpp.Test
{
    public class TestUtilsDbms
    {
        [Fact]
        public void TestDropDatabase()
        {
            ProductionDomainContext productionDomainContext = Dbms.getDbContext();
            if (productionDomainContext.Database.CanConnect() == false)
            {
                productionDomainContext.Database.EnsureCreated();
            }
            
            productionDomainContext.Database.CloseConnection();
            bool wasDropped = Dbms.DropDatabase(Constants.DbName);
            Assert.True(wasDropped, "Db could not be dropped.");
            Thread.Sleep(5000);
            Assert.False(productionDomainContext.Database.CanConnect(),
                "Can still connect to database.");
        }
    }
}