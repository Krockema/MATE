using Master40.Models;
using Master40.Models.DB;
using Microsoft.EntityFrameworkCore;

namespace Master40.Data
{
    public class MasterDBContext : DbContext
    {
        public MasterDBContext(DbContextOptions<MasterDBContext> options) : base(options) { }
        
        public DbSet<Article> Articles { get; set; }
        public DbSet<ArticleBom> ArticleBoms { get; set; }
        public DbSet<ArticleBomPart> ArticleBomParts { get; set; }
        public DbSet<ArticleType> ArticleTypes { get; set; }
        public DbSet<BusinessPartner> BusinessPartners{ get; set; }

        public DbSet<Machine> Machines { get; set; }
        public DbSet<MachineTool> MachineTools { get; set; }
        public DbSet<OperationChart> OperationCharts { get; set; }

        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderPart> OrderParts{ get; set; }
        public DbSet<Purchase> Purchases { get; set; }
        public DbSet<PurchasePart> PurchaseParts { get; set; }
        
        public DbSet<Stock> Stocks { get; set; }
        public DbSet<Unit> Units { get; set; }



        public DbSet<Menu> Menus { get; set; }

        public DbSet<MenuItem> MenuItems { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Article>()
                .ToTable("Article")
                .HasOne(a => a.Stock)
                .WithOne(s => s.Article)
                .HasForeignKey<Stock>(b => b.ArticleForeignKey);

            modelBuilder.Entity<Stock>().ToTable("Stock");
        }
    }
}

