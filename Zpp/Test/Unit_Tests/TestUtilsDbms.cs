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
                productionDomainContext.Database.EnsureDeleted();
                productionDomainContext.Database.EnsureCreated();
            }
            
            productionDomainContext.Database.CloseConnection();
            Dbms.DropDatabase(Constants.DbName);
            Assert.False(productionDomainContext.Database.CanConnect(),
                "Can still connect to database.");
        }
    }
}