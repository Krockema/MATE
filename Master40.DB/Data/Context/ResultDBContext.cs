using System.Linq;
using Master40.DB.Enums;
using Master40.DB.ReportingModel;
using Microsoft.EntityFrameworkCore;

namespace Master40.DB.Data.Context
{
    public class ResultContext : DbContext
    {
        private DbContextOptions<ResultContext> options;

        public static ResultContext GetContext(string resultCon)
        {
            return new ResultContext(options: new DbContextOptionsBuilder<ResultContext>()
                .UseSqlServer(connectionString: resultCon)
                .Options);
        }

        public ResultContext(DbContextOptions<ResultContext> options) : base(options: options) { }

        public DbSet<SimulationConfiguration> SimulationConfigurations { get; set; }
        public DbSet<SimulationWorkschedule> SimulationOperations { get; set; }
        public DbSet<SimulationOrder> SimulationOrders { get; set; }
        public DbSet<Kpi> Kpis { get; set; }
        public DbSet<StockExchange> StockExchanges { get; set; }
    }
}
