using Master40.DB.Data.Context;
using Microsoft.EntityFrameworkCore;

namespace Zpp.Utils
{
    public static class Dbms
    {
      
        public static ProductionDomainContext getDbContext()
        {
         ProductionDomainContext _productionDomainContext;

         // EF inMemory
         // MasterDBContext _inMemmoryContext = new MasterDBContext(new DbContextOptionsBuilder<MasterDBContext>()
         /*_productionDomainContext = new ProductionDomainContext(new DbContextOptionsBuilder<MasterDBContext>()
             .UseInMemoryDatabase(databaseName: "InMemoryDB")
             .Options);*/
         
         // sqlite
         _productionDomainContext = InMemoryContext.CreateInMemoryContext();
            
         
         /*if (Constants.IsWindows)
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
            }*/

            return _productionDomainContext;
        }
    }
}