using System.Linq;
using Master40.DB.Enums;
using Master40.DB.ReportingModel;
using Microsoft.EntityFrameworkCore;

namespace Master40.DB.Data.Context
{
    public class ResultContext : DbContext
    {
        private DbContextOptions<ResultContext> options;

        public ResultContext(DbContextOptions<ResultContext> options) : base(options) { }

        public DbSet<SimulationConfiguration> SimulationConfigurations { get; set; }
        public DbSet<SimulationWorkschedule> SimulationOperations { get; set; }
        public DbSet<SimulationOrder> SimulationOrders { get; set; }
        public DbSet<Kpi> Kpis { get; set; }
        public DbSet<StockExchange> StockExchanges { get; set; }


        public int GetSimulationNumber(int simulationConfigurationId, SimulationType simType)
        {
            var anySim = SimulationOperations
                .Where(x => x.SimulationType == simType && x.SimulationConfigurationId == simulationConfigurationId);
            return anySim.Any() ? anySim.Max(x => x.SimulationNumber) + 1 : 1;
        }
    }
}
