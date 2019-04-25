using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Master40.DB.Data.Context;
using Master40.DB.DataModel;

namespace Master40.DB.Data.Repository
{
    public class OrderDomain : DbContext
    { 
        public OrderDomain(DbContextOptions<OrderDomainContext> options) : base(options) 
        {
            Orders = base.Set<T_CustomerOrder>();
            OrderParts = base.Set<T_CustomerOrderPart>();
            BusinessPartners = base.Set<M_BusinessPartner>();
            Articles = base.Set<M_Article>();
            Stocks = base.Set<M_Stock>();
            Units = base.Set<M_Unit>();
            ArticleTypes = base.Set<M_ArticleType>();
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Ignore<M_ArticleBom>();
            modelBuilder.Ignore<M_Operation>();
            modelBuilder.Ignore<T_ProductionOrder>();
            modelBuilder.Ignore<T_Demand>();
            modelBuilder.Ignore<DemandCustomerOrderPart>();
            modelBuilder.Ignore<T_PurchaseOrder>();

            modelBuilder.Entity<M_Article>()
                .HasOne(a => a.Stock)
                .WithOne(s => s.Article)
                .HasForeignKey<M_Stock>(b => b.ArticleForeignKey);

            modelBuilder.Entity<M_ArticleToBusinessPartner>()
                .HasAlternateKey(x => new { x.ArticleId, x.BusinessPartnerId });

        }
        public DbSet<T_CustomerOrder> Orders { get; set; }
        public DbSet<T_CustomerOrderPart> OrderParts { get; set; }
        public DbSet<M_BusinessPartner> BusinessPartners { get; set; }
        public DbSet<M_Article> Articles { get; set; }
        public DbSet<M_Stock> Stocks { get; set; }
        public DbSet<M_Unit> Units { get; set; }
        public DbSet<M_ArticleType> ArticleTypes { get; set; }
        
    }
}
