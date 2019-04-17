using Master40.DB.Data.Context;
using Microsoft.EntityFrameworkCore;

namespace Zpp.Utils
{
    public static class Dbms
    {
      
        public static ProductionDomainContext getDbContext()
        {
         ProductionDomainContext _productionDomainContext;
            
            if (Constants.IsWindows)
            {
                // Windows
                _productionDomainContext = new ProductionDomainContext(new DbContextOptionsBuilder<MasterDBContext>()
                    .UseSqlServer(
                        Constants.DbConnectionZppWindows)
                    .Options);
            }
            else
            {
                // With Sql Server for Mac/Linux
                _productionDomainContext = new ProductionDomainContext(new DbContextOptionsBuilder<MasterDBContext>()
                    .UseSqlServer(
                        Constants.DbConnectionZppUnix)
                    .Options);
            }

            return _productionDomainContext;
        }
    }
}