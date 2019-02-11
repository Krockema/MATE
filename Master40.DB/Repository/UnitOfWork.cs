using Master40.DB.Data.Context;
using Master40.DB.Models;
using System;
using System.Collections.Generic;

namespace Master40.DB.Repository
{
    public interface IUnitOfWork
    {
        bool InMemory { get; }
        IRepository<ProductionOrder> ProductionOrders { get; }
        
        void Save();
    }


    public class UnitOfWork : IUnitOfWork
    {
        private readonly MasterDBContext _dbContext;

        public bool InMemory { get => _dbContext.InMemory; }
        public UnitOfWork(MasterDBContext dbContext, UnitOfWork orignin = null)
        {
            _dbContext = dbContext;
            if (!_dbContext.InMemory)
            {
                _dbContext.Database.EnsureDeleted();
                _dbContext.Database.EnsureCreated();
            }

            ProductionOrders = new Repository<ProductionOrder>(dbContext, this);
            Articles = new Repository<Article>(dbContext, this);
            ArticleBoms = new Repository<ArticleBom>(dbContext, this);
            ArticleTypes = new Repository<ArticleType>(dbContext, this);
            ArticleToBusinessPartners = new Repository<ArticleToBusinessPartner>(dbContext, this);
            BusinessPartners = new Repository<BusinessPartner>(dbContext, this);
            Demands = new Repository<DemandToProvider>(dbContext, this);
            Machines = new Repository<Machine>(dbContext, this);
            MachineGroups = new Repository<MachineGroup>(dbContext, this);
            MachineTools = new Repository<MachineTool>(dbContext, this);
            SimulationOrders = new Repository<SimulationOrder>(dbContext, this);
            Orders = new Repository<Order>(dbContext, this);
            OrderParts = new Repository<OrderPart>(dbContext, this);
            Purchases = new Repository<Purchase>(dbContext, this);
            ProductionOrders = new Repository<ProductionOrder>(dbContext, this);
            ProductionOrderBoms = new Repository<ProductionOrderBom>(dbContext, this);
            ProductionOrderWorkSchedules = new Repository<ProductionOrderWorkSchedule>(dbContext, this);
            PurchaseParts = new Repository<PurchasePart>(dbContext, this);
            Stocks = new Repository<Stock>(dbContext, this);
            StockExchanges = new Repository<StockExchange>(dbContext, this);
            Units = new Repository<Unit>(dbContext, this);
            WorkSchedules = new Repository<WorkSchedule>(dbContext, this);
            SimulationConfigurations = new Repository<SimulationConfiguration>(dbContext, this);
            SimulationWorkschedules = new Repository<SimulationWorkschedule>(dbContext, this);
            DemandProductionOrderBoms = new Repository<DemandProductionOrderBom>(dbContext, this);
            Kpis = new Repository<Kpi>(dbContext, this);

            if (_dbContext.InMemory)
            {
                CopyDb(orignin);
            }

            // if (ProductionOrders.Count() == 0)
            //     InitalizeDb.BasicSeed(this);
        }
        public IRepository<Article> Articles { get; set; }
        public IRepository<ArticleBom> ArticleBoms { get; set; }
        public IRepository<ArticleType> ArticleTypes { get; set; }
        public IRepository<ArticleToBusinessPartner> ArticleToBusinessPartners { get; set; }
        public IRepository<BusinessPartner> BusinessPartners { get; set; }
        public IRepository<DemandToProvider> Demands { get; set; }
               
        //publiIRepositoryet<DemandToProvider> DemandToProvider { get; set; }
        public IRepository<Machine> Machines { get; set; }
        public IRepository<MachineGroup> MachineGroups { get; set; }
        public IRepository<MachineTool> MachineTools { get; set; }
        public IRepository<SimulationOrder> SimulationOrders { get; set; }
        public IRepository<Order> Orders { get; set; }
        public IRepository<OrderPart> OrderParts { get; set; }
        public IRepository<Purchase> Purchases { get; set; }
        public IRepository<ProductionOrder> ProductionOrders { get; set; }
        public IRepository<ProductionOrderBom> ProductionOrderBoms { get; set; }
        public IRepository<ProductionOrderWorkSchedule> ProductionOrderWorkSchedules { get; set; }
        public IRepository<PurchasePart> PurchaseParts { get; set; }
        public IRepository<Stock> Stocks { get; set; }
        public IRepository<StockExchange> StockExchanges { get; set; }
        public IRepository<Unit> Units { get; set; }
        public IRepository<WorkSchedule> WorkSchedules { get; set; }
        public IRepository<SimulationConfiguration> SimulationConfigurations { get; set; }
        public IRepository<SimulationWorkschedule> SimulationWorkschedules { get; set; }
        public IRepository<DemandProductionOrderBom> DemandProductionOrderBoms { get; set; }
        public IRepository<Kpi> Kpis { get; set; }




        public void Save()
        {
            if (!_dbContext.InMemory)
                _dbContext.SaveChanges();

        }

        private void CopyDb(UnitOfWork origin)
        {

            foreach (var repository in origin.GetType().GetProperties())
            {
                if (repository.Name != "InMemory")
                {
                    var source = origin.GetType().GetProperty(repository.Name).GetValue(origin);
                    var sourceList = ((dynamic)source).ToList();
                    var target = this.GetType().GetProperty(repository.Name).GetValue(this);
                    ((dynamic)target).AddRange(sourceList);
                }
            }
        }

        // public void CreateLinks() {
        //     foreach (var repository in this.GetType().GetProperties())
        //     {
        //         Console.WriteLine(repository.Name);
        //         var repo = this.GetType().GetProperty(repository.Name).GetValue(this);
        //         var elements = ((dynamic)repo).ToList();
        //         foreach (var element in elements)
        //         {
        //             var props = element.GetType().GetProperties();
        //             foreach (var prop in props)
        //             {
        //                 Console.WriteLine(" '-> " + prop.Name);
        //             }
        //             
        //         }
        //     }
        // }
    }
}
