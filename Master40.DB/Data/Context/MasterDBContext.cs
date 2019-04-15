using Master40.DB.DataModel;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace Master40.DB.Data.Context
{
    public class MasterDBContext : DbContext 
    {
        private DbContextOptions<ProductionDomainContext> options;

        public MasterDBContext(DbContextOptions<MasterDBContext> options) : base(options) { }
        [JsonIgnore]
        public bool InMemory { get; internal set; }

        public DbSet<M_Article> Articles { get; set; }
        public DbSet<M_ArticleBom> ArticleBoms { get; set; }
        public DbSet<M_ArticleType> ArticleTypes { get; set; }
        public DbSet<M_ArticleToBusinessPartner> ArticleToBusinessPartners { get; set; }
        public DbSet<M_BusinessPartner> BusinessPartners { get; set; }
        public DbSet<M_Machine> Machines { get; set; }
        public DbSet<M_MachineGroup> MachineGroups { get; set; }
        public DbSet<M_MachineTool> MachineTools { get; set; }
        public DbSet<M_Stock> Stocks { get; set; }
        public DbSet<M_Unit> Units { get; set; }
        public DbSet<M_Operation> Operations { get; set; }
        public DbSet<T_DemandToProvider> DemandToProviders { get; set; }
        //public DbSet<DemandToProvider> DemandToProvider { get; set; }
        public DbSet<T_CustomerOrder> CustomerOrders { get; set; }
        public DbSet<T_CustomerOrderPart> CustomerOrderParts { get; set; }
        public DbSet<T_PurchaseOrder> PurchaseOrders { get; set; }
        public DbSet<T_ProductionOrder> ProductionOrders { get; set; }
        public DbSet<T_PurchaseOrderPart> PurchaseOrderParts { get; set; }
        public DbSet<T_ProductionOrderBom> ProductionOrderBoms { get; set; }
        public DbSet<T_ProductionOrderOperation> ProductionOrderOperations { get; set; }
        public DbSet<T_StockExchange> StockExchanges { get; set; }
        
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {


            modelBuilder.Entity<M_Article>()
                .ToTable("M_Article")
                .HasOne(a => a.Stock)
                .WithOne(s => s.Article)
                .HasForeignKey<M_Stock>(b => b.ArticleForeignKey);

            modelBuilder.Entity<M_ArticleToBusinessPartner>()
                .ToTable("M_ArticleToBusinessPartner")
                .HasAlternateKey(x => new { x.ArticleId, x.BusinessPartnerId });

            modelBuilder.Entity<M_ArticleBom>()
                .ToTable("M_ArticleBom")
                .HasOne(pt => pt.ArticleParent)
                .WithMany(p => p.ArticleBoms)
                .HasForeignKey(pt => pt.ArticleParentId);

            modelBuilder.Entity<M_ArticleBom>()
                .ToTable("M_ArticleBom")
                .HasOne(pt => pt.ArticleChild)
                .WithMany(t => t.ArticleChilds)
                .HasForeignKey(pt => pt.ArticleChildId);

            modelBuilder.Entity<M_Operation>()
                .ToTable("M_Operation")
                .HasOne(m => m.MachineTool)
                .WithMany(m => m.WorkSchedules)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<M_ArticleType>()
                .ToTable("M_ArticleType");

            modelBuilder.Entity<M_BusinessPartner>()
                .ToTable("M_BusinessPartner");

            modelBuilder.Entity<M_ArticleToBusinessPartner>()
                .ToTable("M_ArticleToBusinessPartner");

            modelBuilder.Entity<M_Machine>()
                .ToTable("M_Machine");
            
            modelBuilder.Entity<M_MachineGroup>()
                .ToTable("M_MachineGroup");
            
            modelBuilder.Entity<M_MachineTool>()
                .ToTable("M_MachineTool");

            modelBuilder.Entity<M_Stock>()
                .ToTable("M_Stock");

            modelBuilder.Entity<M_Unit>()
                .ToTable("M_Unit");

            modelBuilder.Entity<M_Operation>()
                .ToTable("M_Operation");

            modelBuilder.Entity<T_PurchaseOrder>()
                .ToTable("T_PurchaseOrder");

            modelBuilder.Entity<T_PurchaseOrderPart>()
                .ToTable("T_PurchaseOrderPart");

            modelBuilder.Entity<T_CustomerOrder>()
                .ToTable("T_CustomerOrder");

            modelBuilder.Entity<T_CustomerOrderPart>()
                .ToTable("T_CustomerOrderPart");
           
            modelBuilder.Entity<T_ProductionOrder>()
                .ToTable("T_ProductionOrder");

            modelBuilder.Entity<T_StockExchange>()
                .ToTable("T_StockExchange");
            
            modelBuilder.Entity<T_ProductionOrderBom>()
                .ToTable("T_ProductionOrderBom")
                .HasOne(p => p.ProductionOrderParent)
                .WithMany(b => b.ProductionOrderBoms)
                .HasForeignKey(fk => fk.ProductionOrderParentId);
            
            modelBuilder.Entity<T_ProductionOrderOperation>()
                .ToTable("T_ProductionOrderOperation")
                .HasOne(m => m.MachineGroup)
                .WithMany(m => m.ProductionOrderWorkSchedules)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<T_ProductionOrderOperation>()
                .ToTable("T_ProductionOrderOperation")
                .HasOne(m => m.Machine)
                .WithMany(m => m.ProductionOrderWorkSchedules)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<T_DemandToProvider>()
                .ToTable("T_DemandToProvider")
                .HasOne(d => d.DemandRequester)
                .WithMany(r => r.DemandProvider)
                .HasForeignKey(fk => fk.DemandRequesterId);

            modelBuilder.Entity<DemandProviderProductionOrder>()
                .HasOne(d => d.ProductionOrder)
                .WithMany(r => r.DemandProviderProductionOrders)
                .HasForeignKey(fk => fk.ProductionOrderId)
                .OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<DemandProviderPurchasePart>()
                .HasOne(d => d.PurchasePart)
                .WithMany(r => r.DemandProviderPurchaseParts)
                .HasForeignKey(fk => fk.PurchasePartId)
                .OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<DemandProviderStock>()
                .HasOne(d => d.Stock)
                .WithMany(r => r.DemandProviderStocks)
                .HasForeignKey(fk => fk.StockId)
                .OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<DemandStock>()
                .HasOne(d => d.Stock)
                .WithMany(r => r.DemandStocks)
                .HasForeignKey(fk => fk.StockId)
                .OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<DemandProductionOrderBom>()
                .HasOne(d => d.ProductionOrderBom)
                .WithMany(r => r.DemandProductionOrderBoms)
                .HasForeignKey(fk => fk.ProductionOrderBomId)
                .OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<DemandOrderPart>()
                .HasOne(d => d.OrderPart)
                .WithMany(r => r.DemandOrderParts)
                .HasForeignKey(fk => fk.OrderPartId)
                .OnDelete(DeleteBehavior.Restrict);

        }
    }
}