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
        public DbSet<ArticleType> ArticleTypes { get; set; }
        public DbSet<BusinessPartner> BusinessPartners{ get; set; }

        public DbSet<Machine> Machines { get; set; }
        public DbSet<MachineTool> MachineTools { get; set; }
        public DbSet<WorkSchedule> OperationCharts { get; set; }

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

            modelBuilder.Entity<ArticleToWorkSchedule>()
                .HasKey(a => new { a.ArticleId, a.WorkScheduleId });

            modelBuilder.Entity<Stock>().ToTable("Stock");

            modelBuilder.Entity<ArticleBom>()
                .HasOne(pt => pt.ArticleParent)
                .WithMany(p => p.ArticleBoms)
                .HasForeignKey(pt => pt.ArticleParentId);

            modelBuilder.Entity<ArticleBom>()
                .HasOne(pt => pt.ArticleChild)
                .WithMany(t => t.ArticleChilds)
                .HasForeignKey(pt => pt.ArticleChildId);

            modelBuilder.Entity<ProductionOrderBom>()
                .HasOne(pt => pt.ParentProductionOrder)
                .WithMany(p => p.ProductionOrderBoms)
                .HasForeignKey(pt => pt.ParentProductionOrderId);

            modelBuilder.Entity<ProductionOrderBom>()
                .HasOne(pt => pt.ProductionOrder)
                .WithMany(t => t.ChildProductionOrderBoms)
                .HasForeignKey(pt => pt.ProductionOrderId);
        }
    }
}

