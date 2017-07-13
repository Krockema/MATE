using System.Linq;
using Master40.DB.Data.Repository;
using Master40.DB.Models;
using Microsoft.EntityFrameworkCore;

namespace Master40.DB.Data.Context
{
    public class CopyContext : MasterDBContext
    {
        public CopyContext(DbContextOptions<MasterDBContext> options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<ArticleType>().Property(p => p.Id).ValueGeneratedNever();
            modelBuilder.Entity<Article>().Property(p => p.Id).ValueGeneratedNever();
            modelBuilder.Entity<ArticleBom>().Property(p => p.Id).ValueGeneratedNever();
            modelBuilder.Entity<ArticleToBusinessPartner>().Property(p => p.Id).ValueGeneratedNever();
            modelBuilder.Entity<BusinessPartner>().Property(p => p.Id).ValueGeneratedNever();
            modelBuilder.Entity<DemandToProvider>().Property(p => p.Id).ValueGeneratedNever();
            modelBuilder.Entity<Kpi>().Property(p => p.Id).ValueGeneratedNever();
            modelBuilder.Entity<Machine>().Property(p => p.Id).ValueGeneratedNever();
            modelBuilder.Entity<MachineTool>().Property(p => p.Id).ValueGeneratedNever();
            modelBuilder.Entity<MachineGroup>().Property(p => p.Id).ValueGeneratedNever();
            modelBuilder.Entity<Order>().Property(p => p.Id).ValueGeneratedNever();
            modelBuilder.Entity<OrderPart>().Property(p => p.Id).ValueGeneratedNever();
            modelBuilder.Entity<ProductionOrder>().Property(p => p.Id).ValueGeneratedNever();
            modelBuilder.Entity<ProductionOrderBom>().Property(p => p.Id).ValueGeneratedNever();
            modelBuilder.Entity<ProductionOrderWorkSchedule>().Property(p => p.Id).ValueGeneratedNever();
            modelBuilder.Entity<Purchase>().Property(p => p.Id).ValueGeneratedNever();
            modelBuilder.Entity<PurchasePart>().Property(p => p.Id).ValueGeneratedNever();
            modelBuilder.Entity<Stock>().Property(p => p.Id).ValueGeneratedNever();
            modelBuilder.Entity<Unit>().Property(p => p.Id).ValueGeneratedNever();
            modelBuilder.Entity<WorkSchedule>().Property(p => p.Id).ValueGeneratedNever();
        }
    }
}
