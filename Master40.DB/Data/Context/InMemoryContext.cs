using EfCore.InMemoryHelpers;
using EntityFrameworkCore.Cacheable;
using Master40.DB.Data.Helper;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace Master40.DB.Data.Context
{
    public class InMemoryContext : ProductionDomainContext
    {
        public InMemoryContext(DbContextOptions<MasterDBContext> options) : base(options: options)
        {
        }
        /*
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            /*
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
        */
        public static ProductionDomainContext CreateInMemoryContext()
        {
            // In-memory database only exists while the connection is open
            var options = new DbContextOptionsBuilder<MasterDBContext>();
            options.UseSecondLevelCache();

            InMemoryContext _inMemmoryContext = InMemoryContextBuilder.Build<InMemoryContext>(builder: options);

            _inMemmoryContext.Database.EnsureCreated();

            return _inMemmoryContext;
        }

        public static ProductionDomainContext CreateInMemoryContext_Old()
        {
            // In-memory database only exists while the connection is open
            var connectionStringBuilder = new SqliteConnectionStringBuilder {DataSource = ":memory:"};
            var connection = new SqliteConnection(connectionString: connectionStringBuilder.ToString());

            // create OptionsBuilder with InMemmory Context
            var builder = new DbContextOptionsBuilder<MasterDBContext>();
            builder.UseSqlite(connection: connection);
            //builder.UseSecondLevelCache();
            var c = new ProductionDomainContext(options: builder.Options);

            c.Database.OpenConnection();
            c.Database.EnsureCreated();

            return c;
        }

        public static void LoadData(ProductionDomainContext source, ProductionDomainContext target)
        {
            foreach (var item in source.ArticleTypes)
            {
                target.ArticleTypes.Add(entity: item.CopyProperties());
            }
            target.SaveChanges();

            foreach (var item in source.Units)
            {
                target.Units.Add(entity: item.CopyProperties());
            }
            target.SaveChanges();

            foreach (var item in source.ResourceSkills)
            {
                target.ResourceSkills.Add(entity: item.CopyProperties());
            }
            target.SaveChanges();

            foreach (var item in source.ResourceTools)
            {
                target.ResourceTools.Add(entity: item.CopyProperties());
            }
            target.SaveChanges();

            foreach (var item in source.Resources)
            {
                target.Resources.Add(entity: item.CopyProperties());
            }
            target.SaveChanges();

            foreach (var item in source.ResourceSetups)
            {
                target.ResourceSetups.Add(entity: item.CopyProperties());
            }
            target.SaveChanges();

            foreach (var item in source.Articles)
            {
                target.Articles.Add(entity: item.CopyProperties());
            }
            target.SaveChanges();

            foreach (var item in source.Stocks)
            {
                target.Stocks.Add(entity: item.CopyProperties());
            }
            target.SaveChanges();

            foreach (var item in source.Operations)
            {
                target.Operations.Add(entity: item.CopyProperties());
            }
            target.SaveChanges();

            foreach (var item in source.ArticleBoms)
            {
                target.ArticleBoms.Add(entity: item.CopyProperties());
            }
            target.SaveChanges();

            foreach (var item in source.BusinessPartners)
            {
                target.BusinessPartners.Add(entity: item.CopyProperties());
            }
            target.SaveChanges();

            foreach (var item in source.ArticleToBusinessPartners)
            {
                target.ArticleToBusinessPartners.Add(entity: item.CopyProperties());
            }
            target.SaveChanges();

            foreach (var item in source.CustomerOrders)
            {
                target.CustomerOrders.Add(entity: item.CopyProperties());
            }
            target.SaveChanges();
            
            foreach (var item in source.CustomerOrderParts)
            {
                target.CustomerOrderParts.Add(entity: item.CopyProperties());
            }
            target.SaveChanges();

            foreach (var item in source.ProductionOrders)
            {
                target.ProductionOrders.Add(entity: item.CopyProperties());
            }
            target.SaveChanges();
        }

        public static void SaveData(ResultContext source, ResultContext target)
        {
            foreach (var item in source.Kpis)
            {
                target.Kpis.Add(entity: item.CopyProperties());
            }
            target.SaveChanges();

            foreach (var item in source.SimulationOperations)
            {
                target.SimulationOperations.Add(entity: item.CopyProperties());
            }
            target.SaveChanges();

            foreach (var item in source.StockExchanges)
            {
                target.StockExchanges.Add(entity: item.CopyProperties());
            }
            target.SaveChanges();
        }
    }
}
