using Mate.DataCore.ReportingModel;
using Microsoft.EntityFrameworkCore;

namespace Mate.DataCore.Data.Context
{
    public class MateResultDb : DbContext
    {
        public static MateResultDb GetContext(string resultCon)
        {
            return new MateResultDb(options: new DbContextOptionsBuilder<MateResultDb>()
                .UseSqlServer(connectionString: resultCon)
                .Options);
        }

        protected override void OnConfiguring(DbContextOptionsBuilder options)
            => options.EnableSensitiveDataLogging();

        public MateResultDb(DbContextOptions<MateResultDb> options) : base(options: options) { }
        public MateResultDb() {}

        public DbSet<SimulationConfiguration> SimulationConfigurations { get; set; }
        public DbSet<ConfigurationItem> ConfigurationItems { get; set; }
        public DbSet<ConfigurationRelation> ConfigurationRelations { get; set; }
        public DbSet<TaskItem> TaskItems { get; set; } //represents the task items from Resource-Agents
        public DbSet<Job> SimulationJobs { get; set; } //represents each job (buckets and operations) from Job-Agent
        public DbSet<Setup> SimulationResourceSetups { get; set; }
        public DbSet<SimulationOrder> SimulationOrders { get; set; }
        public DbSet<Kpi> Kpis { get; set; }
        public DbSet<StockExchange> StockExchanges { get; set; }
        public DbSet<SimulationConfig> SimulationConfigs { get; set; }
        public DbSet<SimulationMeasurement> SimulationMeasurements { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ConfigurationRelation>()
                .HasKey(t => new { t.ParentItemId, t.ChildItemId });

            modelBuilder.Entity<ConfigurationRelation>()
                .HasOne(navigationExpression: pt => pt.ParentItem)
                .WithMany(navigationExpression: p => p.ParentItems)
                .HasForeignKey(foreignKeyExpression: pt => pt.ParentItemId)
                .OnDelete(deleteBehavior: DeleteBehavior.Restrict);

            modelBuilder.Entity<ConfigurationRelation>()
                .HasOne(navigationExpression: pt => pt.ChildItem)
                .WithMany(navigationExpression: p => p.ChildItems)
                .HasForeignKey(foreignKeyExpression: pt => pt.ChildItemId)
                .OnDelete(deleteBehavior: DeleteBehavior.Restrict);
            modelBuilder.Entity<SimulationConfig>()
                .ToTable("SimulationConfig");
        }
    }
}
