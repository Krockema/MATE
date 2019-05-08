﻿using Microsoft.EntityFrameworkCore;
using Master40.DB.Models;

namespace Master40.DB.Data.Context
{
    public class MasterDBContext : DbContext 
    {
        private DbContextOptions<ProductionDomainContext> options;

        public MasterDBContext(DbContextOptions<MasterDBContext> options) : base(options) { }


        public DbSet<Article> Articles { get; set; }
        public DbSet<ArticleBom> ArticleBoms { get; set; }
        public DbSet<ArticleType> ArticleTypes { get; set; }
        public DbSet<ArticleToBusinessPartner> ArticleToBusinessPartners { get; set; }
        public DbSet<BusinessPartner> BusinessPartners { get; set; }
        public DbSet<DemandToProvider> Demands { get; set; }

        //public DbSet<DemandToProvider> DemandToProvider { get; set; }
        public DbSet<Machine> Machines { get; set; }
        public DbSet<MachineGroup> MachineGroups { get; set; }
        public DbSet<MachineTool> MachineTools { get; set; }
        public DbSet<SimulationOrder> SimulationOrders  { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderPart> OrderParts { get; set; }
        public DbSet<Purchase> Purchases { get; set; }
        public DbSet<ProductionOrder> ProductionOrders { get; set; }
        public DbSet<ProductionOrderBom> ProductionOrderBoms { get; set; }
        public DbSet<ProductionOrderWorkSchedule> ProductionOrderWorkSchedules { get; set; }
        public DbSet<PurchasePart> PurchaseParts { get; set; }
        public DbSet<Stock> Stocks { get; set; }
        public DbSet<StockExchange> StockExchanges { get; set; }
        public DbSet<Unit> Units { get; set; }
        public DbSet<WorkSchedule> WorkSchedules { get; set; }
        public DbSet<SimulationConfiguration> SimulationConfigurations { get; set; }
        public DbSet<SimulationWorkschedule> SimulationWorkschedules { get; set; }
        public DbSet<DemandProductionOrderBom> DemandProductionOrderBoms { get; set; }
        public DbSet<Kpi> Kpis { get; set; }
        public DbSet<Mapping> Mappings { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Article>()
                .HasOne(a => a.Stock)
                .WithOne(s => s.Article)
                .HasForeignKey<Stock>(b => b.ArticleForeignKey);

            modelBuilder.Entity<ArticleToBusinessPartner>()
                .HasAlternateKey(x => new { x.ArticleId, x.BusinessPartnerId });

            modelBuilder.Entity<ArticleBom>()
                .HasOne(pt => pt.ArticleParent)
                .WithMany(p => p.ArticleBoms)
                .HasForeignKey(pt => pt.ArticleParentId);

            modelBuilder.Entity<ArticleBom>()
                .HasOne(pt => pt.ArticleChild)
                .WithMany(t => t.ArticleChilds)
                .HasForeignKey(pt => pt.ArticleChildId);

            modelBuilder.Entity<ProductionOrderBom>()
                .HasOne(p => p.ProductionOrderParent)
                .WithMany(b => b.ProductionOrderBoms)
                .HasForeignKey(fk => fk.ProductionOrderParentId);
            
            modelBuilder.Entity<ProductionOrderWorkSchedule>()
                .HasOne(m => m.MachineGroup)
                .WithMany(m => m.ProductionOrderWorkSchedules)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<ProductionOrderWorkSchedule>()
                .HasOne(m => m.Machine)
                .WithMany(m => m.ProductionOrderWorkSchedules)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<DemandToProvider>()
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
            modelBuilder.Entity<Mapping>()
                .ToTable("M_Mapping");
        }
    }
}