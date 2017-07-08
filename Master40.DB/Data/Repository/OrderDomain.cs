using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Master40.DB.Data.Context;
using Master40.DB.Models;

namespace Master40.DB.Data.Repository
{
    public class OrderDomain : DbContext
    { 
        public OrderDomain(DbContextOptions<OrderDomainContext> options) : base(options) 
        {
            Orders = base.Set<Order>();
            OrderParts = base.Set<OrderPart>();
            BusinessPartners = base.Set<BusinessPartner>();
            Articles = base.Set<Article>();
            Stocks = base.Set<Stock>();
            Units = base.Set<Unit>();
            ArticleTypes = base.Set<ArticleType>();
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Ignore<ArticleBom>();
            modelBuilder.Ignore<WorkSchedule>();
            modelBuilder.Ignore<ProductionOrder>();
            modelBuilder.Ignore<DemandToProvider>();
            modelBuilder.Ignore<DemandOrderPart>();
            modelBuilder.Ignore<Purchase>();

            modelBuilder.Entity<Article>()
                .HasOne(a => a.Stock)
                .WithOne(s => s.Article)
                .HasForeignKey<Stock>(b => b.ArticleForeignKey);

            modelBuilder.Entity<ArticleToBusinessPartner>()
                .HasAlternateKey(x => new { x.ArticleId, x.BusinessPartnerId });

        }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderPart> OrderParts { get; set; }
        public DbSet<BusinessPartner> BusinessPartners { get; set; }
        public DbSet<Article> Articles { get; set; }
        public DbSet<Stock> Stocks { get; set; }
        public DbSet<Unit> Units { get; set; }
        public DbSet<ArticleType> ArticleTypes { get; set; }
        
    }
}
