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
        public DbSet<ArticleBomItem> ArticleBomItems { get; set; }
        public DbSet<ArticleType> ArticleTypes { get; set; }
        public DbSet<ArticleToWorkSchedule> ArticleToWorkSchedule { get; set; }
        public DbSet<BusinessPartner> BusinessPartners{ get; set; }
        public DbSet<Demand> Demands { get; set; }
        public DbSet<DemandOrder> DemandOrders { get; set; }
        public DbSet<DemandPurchase> DemandPurchases { get; set; }
        public DbSet<DemandStock> DemandStock { get; set; }
        public DbSet<Machine> Machines { get; set; }
        public DbSet<MachineGroup> MachineGroups { get; set; }
        public DbSet<MachineTool> MachineTools { get; set; }
        public DbSet<WorkSchedule> OperationCharts { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderPart> OrderParts{ get; set; }
        public DbSet<Purchase> Purchases { get; set; }
        public DbSet<ProductionOrder> ProductionOrders { get; set; }
        public DbSet<ProductionOrderBom> ProductionOrderBoms { get; set; }
        public DbSet<ProductionOrderBomItem> ProductionOrderBomItems { get; set; }
        public DbSet<ProductionOrderToProductionOrderWorkSchedule> ProductionOrderToProductionOrderWorkSchedules { get; set; }
        public DbSet<ProductionOrderWorkSchedule> ProductionOrderWorkSchedule { get; set; }
        public DbSet<PurchasePart> PurchaseParts { get; set; }
        public DbSet<Stock> Stocks { get; set; }
        public DbSet<Unit> Units { get; set; }
        public DbSet<WorkSchedule> WorkSchedules { get; set; }




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
                .HasOne(a => a.Article)
                .WithMany(b => b.ArticleBoms)
                .HasForeignKey(fk => fk.ArticleId);

            modelBuilder.Entity<ArticleBomItem>()
                .HasOne(a => a.Article)
                .WithMany(b => b.ArticleBomItems)
                .HasForeignKey(fk => fk.ArticleId)
                .OnDelete(Microsoft.EntityFrameworkCore.Metadata.DeleteBehavior.Restrict);

            modelBuilder.Entity<ProductionOrderBom>()
                .HasOne(p => p.ProductionOrder)
                .WithMany(b => b.ProductionOrderBoms)
                .HasForeignKey(fk => fk.ProductionOrderId);

            modelBuilder.Entity<ProductionOrderBomItem>()
                .HasOne(p => p.ProductionOrder)
                .WithMany(b => b.ProductionOrderBomItems)
                .HasForeignKey(fk => fk.ProductionOrderId)
                .OnDelete(Microsoft.EntityFrameworkCore.Metadata.DeleteBehavior.Restrict);

            /*
            modelBuilder.Entity<ArticleBomItem>()
    .HasOne(pt => pt.Article)
    .WithMany(p => p.ArticleBomItems)
    .HasForeignKey(pt => pt.ArticleId);

            modelBuilder.Entity<ArticleBomItem>()
                .HasOne(pt => pt.ArticleBom)
                .WithMany(t => t.ArticleBomItems)
                .HasForeignKey(pt => pt.ArticleBomId);

            modelBuilder.Entity<ProductionOrderBomItem>()
                .HasOne(pt => pt.ProductionOrder)
                .WithMany(p => p.ProductionOrderBomItems)
                .HasForeignKey(pt => pt.ProductionOrderId);

            modelBuilder.Entity<ProductionOrderBomItem>()
                .HasOne(pt => pt.ProductionOderBom)
                .WithMany(t => t.ProductionOrderBomItems)
                .HasForeignKey(pt => pt.ProductionOderBomId);
                */
        }
    }
}

