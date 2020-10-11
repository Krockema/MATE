using Master40.DB.GeneratorModel;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace Master40.DB.Data.Context
{
    public class DataGeneratorContext : DbContext
    {
        public static DataGeneratorContext GetContext(string connectionString)
        {
            return new DataGeneratorContext(new DbContextOptionsBuilder<DataGeneratorContext>()
                .UseSqlServer(connectionString: connectionString)
                .Options);
        }

        public DataGeneratorContext(DbContextOptions<DataGeneratorContext> options) : base(options: options)
        {
        }

        [JsonIgnore] public bool InMemory { get; internal set; }
        public DbSet<InputParameter> Approaches { get; set; }
        public DbSet<TransitionMatrix> TransitionMatrices { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<InputParameter>()
                .ToTable("Approach");
            modelBuilder.Entity<TransitionMatrix>()
                .ToTable(name: "TransitionMatrix")
                .HasOne(tm => tm.Approach)
                .WithMany(a => a.TransitionMatrix)
                .HasForeignKey(tm => tm.ApproachId);

        }

        protected override void OnConfiguring(DbContextOptionsBuilder options)
            => options.EnableSensitiveDataLogging();
    }
}