using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Master40.DB.Data.Context;

namespace Master40.DB.Data.Repository
{
    public class SchedulingDomain : MasterDBContext
    { 
        public SchedulingDomain(DbContextOptions<MasterDBContext> options) : base(options)
        {
            /*
            Machine = base.Set<Machine>();
            ProductionOrderBom = base.Set<ProductionOrderBom>();
            ProductionOrderWorkSchedule = base.Set<ProductionOrderWorkSchedule>();
            DemandToProvider = base.Set<DemandToProvider>();
            OrderPart = base.Set<OrderPart>();
            ProductionOrder = base.Set<ProductionOrder>();
            */
            
        }

        /*public DbSet<Machine> Machine { get; set; }
        public DbSet<ProductionOrderBom> ProductionOrderBom { get; set; }
        public DbSet<ProductionOrderWorkSchedule> ProductionOrderWorkSchedule { get; set; }
        public DbSet<DemandToProvider> DemandToProvider { get; set; }
        public DbSet<OrderPart> OrderPart { get; set; }
        
        public DbSet<ProductionOrder> ProductionOrder { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Ignore<MachineGroup>();
            modelBuilder.Ignore<MachineTool>();
           

            modelBuilder.Entity<ProductionOrderBom>()
            .HasOne(p => p.ProductionOrderParent)
            .WithMany(b => b.ProductionOrderBoms)
            .HasForeignKey(fk => fk.ProductionOrderParentId);

            modelBuilder.Entity<ProductionOrderBom>()
            .HasOne(p => p.ProductionOrderChild)
            .WithMany(b => b.ProdProductionOrderBomChilds)
            .HasForeignKey(fk => fk.ProductionOrderChildId)
            .OnDelete(Microsoft.EntityFrameworkCore.Metadata.DeleteBehavior.Restrict);

            modelBuilder.Entity<ProductionOrderWorkSchedule>()
            .HasOne(m => m.MachineGroup)
            .WithMany(m => m.ProductionOrderWorkSchedules)
            .OnDelete(Microsoft.EntityFrameworkCore.Metadata.DeleteBehavior.Restrict);

            modelBuilder.Entity<ProductionOrderWorkSchedule>()
            .HasOne(m => m.Machine)
            .WithMany(m => m.ProductionOrderWorkSchedules)
            .OnDelete(Microsoft.EntityFrameworkCore.Metadata.DeleteBehavior.Restrict);

            modelBuilder.Entity<DemandToProvider>()
            .HasOne(d => d.DemandRequester)
            .WithMany(r => r.DemandProvider)
            .HasForeignKey(fk => fk.DemandRequesterId);

            modelBuilder.Entity<DemandProviderProductionOrder>()
            .HasOne(d => d.ProductionOrder)
            .WithMany(r => r.DemandProviderProductionOrders)
            .HasForeignKey(fk => fk.ProductionOrderId)
            .OnDelete(Microsoft.EntityFrameworkCore.Metadata.DeleteBehavior.Restrict);
            modelBuilder.Entity<DemandProviderPurchasePart>()
            .HasOne(d => d.PurchasePart)
            .WithMany(r => r.DemandProviderPurchaseParts)
            .HasForeignKey(fk => fk.PurchasePartId)
            .OnDelete(Microsoft.EntityFrameworkCore.Metadata.DeleteBehavior.Restrict);
            modelBuilder.Entity<DemandProviderStock>()
            .HasOne(d => d.Stock)
            .WithMany(r => r.DemandProviderStocks)
            .HasForeignKey(fk => fk.StockId)
            .OnDelete(Microsoft.EntityFrameworkCore.Metadata.DeleteBehavior.Restrict);
            modelBuilder.Entity<DemandStock>()
            .HasOne(d => d.Stock)
            .WithMany(r => r.DemandStocks)
            .HasForeignKey(fk => fk.StockId)
            .OnDelete(Microsoft.EntityFrameworkCore.Metadata.DeleteBehavior.Restrict);
            modelBuilder.Entity<DemandProductionOrderBom>()
            .HasOne(d => d.ProductionOrderBom)
            .WithMany(r => r.DemandProductionOrderBoms)
            .HasForeignKey(fk => fk.ProductionOrderBomId)
            .OnDelete(Microsoft.EntityFrameworkCore.Metadata.DeleteBehavior.Restrict);
            modelBuilder.Entity<DemandOrderPart>()
            .HasOne(d => d.OrderPart)
            .WithMany(r => r.DemandOrderParts)
            .HasForeignKey(fk => fk.OrderPartId)
            .OnDelete(Microsoft.EntityFrameworkCore.Metadata.DeleteBehavior.Restrict);
            }
            */

        
    }
}
