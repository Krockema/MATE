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
        public DbSet<M_Resource> Resources { get; set; }
        public DbSet<M_MachineGroup> MachineGroups { get; set; }
        public DbSet<M_ResourceTool> ResourceTools { get; set; }
        public DbSet<M_ResourceSkill> ResourceSkills { get; set; }
        public DbSet<M_ResourceToResourceTool> ResourceToResourceTools { get; set; }
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
        public DbSet<T_Demand> Demands { get; set; }
        public DbSet<T_Provider> Providers { get; set; }

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
                .HasForeignKey(pt => pt.ArticleParentId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<M_ArticleBom>()
                .ToTable("M_ArticleBom")
                .HasOne(pt => pt.ArticleChild)
                .WithMany(t => t.ArticleChilds)
                .HasForeignKey(pt => pt.ArticleChildId);

            modelBuilder.Entity<M_ArticleBom>()
                .ToTable("M_ArticleBom")
                .HasOne(pt => pt.Operation)
                .WithMany(t => t.ArticleBoms)
                .HasForeignKey(pt => pt.OperationId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<M_Operation>()
                .ToTable("M_Operation")
                .HasOne(m => m.ResourceSkill);

            modelBuilder.Entity<M_ArticleType>()
                .ToTable("M_ArticleType");

            modelBuilder.Entity<M_BusinessPartner>()
                .ToTable("M_BusinessPartner");

            modelBuilder.Entity<M_ArticleToBusinessPartner>()
                .ToTable("M_ArticleToBusinessPartner");

            modelBuilder.Entity<M_MachineGroup>()
                .ToTable("M_MachineGroup");

            modelBuilder.Entity<M_Resource>()
                .ToTable("M_Resource");

            modelBuilder.Entity<M_ResourceTool>()
                .ToTable("M_ResourceTool");

            modelBuilder.Entity<M_ResourceSkill>()
                .ToTable("M_ResourceSkill");

            modelBuilder.Entity<M_ResourceToResourceTool>()
                .ToTable("M_ResourceToResourceTool")
                .HasOne(re => re.Resource)
                .WithMany(r => r.ResourceToResourceTools)
                .HasForeignKey(re => re.ResourceId);

            modelBuilder.Entity<M_ResourceToResourceTool>()
                .ToTable("M_ResourceToResourceTool")
                .HasOne(re => re.ResourceTool)
                .WithMany(r => r.ResourceToResourceTools)
                .HasForeignKey(re => re.ResourceToolId);

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
                .HasForeignKey(fk => fk.ProductionOrderParentId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<T_ProductionOrderBom>()
                .ToTable("T_ProductionOrderBom")
                .HasOne(p => p.ProductionOrderOperation)
                .WithMany(b => b.ProductionOrderBoms)
                .HasForeignKey(fk => fk.ProductionOrderOperationId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<T_ProductionOrderOperation>()
                .ToTable("T_ProductionOrderOperation")
                .HasOne(m => m.MachineGroup)
                .WithMany(m => m.ProductionOrderWorkSchedules)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<T_ProductionOrderOperation>()
                .ToTable("T_ProductionOrderOperation")
                .HasOne(m => m.Resource)
                .WithMany(m => m.ProductionOrderWorkSchedules)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<T_Demand>()
                .ToTable("T_Demand");
            modelBuilder.Entity<T_Provider>()
                .ToTable("T_Provider");
            modelBuilder.Entity<T_DemandToProvider>()
                .ToTable("T_DemandToProvider");
        }
    }
}