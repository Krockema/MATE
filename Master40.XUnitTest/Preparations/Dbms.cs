using Master40.DB.Data.Context;
using Microsoft.EntityFrameworkCore;

namespace Master40.XUnitTest.Preparations
{
    public static class Dbms
    {
        public static ProductionDomainContext getDbContext()
        {
            ProductionDomainContext _productionDomainContext;

            if (ConnectionsStrings.IsWindows)
            {
                // Windows
                _productionDomainContext = new ProductionDomainContext(options: new DbContextOptionsBuilder<MasterDBContext>()
                    .UseSqlServer(
                        connectionString: ConnectionsStrings.DbConnectionZppWindows)
                    .Options);
            }
            else
            {
                // With Sql Server for Mac/Linux
                _productionDomainContext = new ProductionDomainContext(options: new DbContextOptionsBuilder<MasterDBContext>()
                    .UseSqlServer(
                        connectionString: ConnectionsStrings.DbConnectionZppUnix)
                    .Options);
            }

            return _productionDomainContext;
        }

        public static string getDbContextString()
        {
            if (ConnectionsStrings.IsWindows)
            {
                return ConnectionsStrings.DbConnectionZppWindows;
            }
            else
            {
                return ConnectionsStrings.DbConnectionZppUnix;
            }
        }
    }
}