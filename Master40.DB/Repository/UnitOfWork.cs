using Master40.DB.Data.Context;
using System;
using System.Collections.Generic;
using Master40.DB.DataModel;
using Master40.DB.ReportingModel;

namespace Master40.DB.Repository
{
    public interface IUnitOfWork
    {
        bool InMemory { get; }
        IRepository<T_ProductionOrder> ProductionOrders { get; }
        
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

            ProductionOrders = new Repository<T_ProductionOrder>(dbContext, this);
            Articles = new Repository<M_Article>(dbContext, this);
            ArticleBoms = new Repository<M_ArticleBom>(dbContext, this);
            ArticleTypes = new Repository<M_ArticleType>(dbContext, this);
            ArticleToBusinessPartners = new Repository<M_ArticleToBusinessPartner>(dbContext, this);
            BusinessPartners = new Repository<M_BusinessPartner>(dbContext, this);
            Demands = new Repository<T_Demand>(dbContext, this);
            Machines = new Repository<M_Machine>(dbContext, this);
            MachineGroups = new Repository<M_MachineGroup>(dbContext, this);
            MachineTools = new Repository<M_MachineTool>(dbContext, this);
            SimulationOrders = new Repository<SimulationOrder>(dbContext, this);
            Orders = new Repository<T_CustomerOrder>(dbContext, this);
            OrderParts = new Repository<T_CustomerOrderPart>(dbContext, this);
            Purchases = new Repository<T_PurchaseOrder>(dbContext, this);
            ProductionOrders = new Repository<T_ProductionOrder>(dbContext, this);
            ProductionOrderBoms = new Repository<T_ProductionOrderBom>(dbContext, this);
            ProductionOrderWorkSchedules = new Repository<T_ProductionOrderOperation>(dbContext, this);
            PurchaseParts = new Repository<T_PurchaseOrderPart>(dbContext, this);
            Stocks = new Repository<M_Stock>(dbContext, this);
            StockExchanges = new Repository<T_StockExchange>(dbContext, this);
            Units = new Repository<M_Unit>(dbContext, this);
            WorkSchedules = new Repository<M_Operation>(dbContext, this);
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
        public IRepository<M_Article> Articles { get; set; }
        public IRepository<M_ArticleBom> ArticleBoms { get; set; }
        public IRepository<M_ArticleType> ArticleTypes { get; set; }
        public IRepository<M_ArticleToBusinessPartner> ArticleToBusinessPartners { get; set; }
        public IRepository<M_BusinessPartner> BusinessPartners { get; set; }
        public IRepository<T_Demand> Demands { get; set; }
               
        //publiIRepositoryet<DemandToProvider> DemandToProvider { get; set; }
        public IRepository<M_Machine> Machines { get; set; }
        public IRepository<M_MachineGroup> MachineGroups { get; set; }
        public IRepository<M_MachineTool> MachineTools { get; set; }
        public IRepository<SimulationOrder> SimulationOrders { get; set; }
        public IRepository<T_CustomerOrder> Orders { get; set; }
        public IRepository<T_CustomerOrderPart> OrderParts { get; set; }
        public IRepository<T_PurchaseOrder> Purchases { get; set; }
        public IRepository<T_ProductionOrder> ProductionOrders { get; set; }
        public IRepository<T_ProductionOrderBom> ProductionOrderBoms { get; set; }
        public IRepository<T_ProductionOrderOperation> ProductionOrderWorkSchedules { get; set; }
        public IRepository<T_PurchaseOrderPart> PurchaseParts { get; set; }
        public IRepository<M_Stock> Stocks { get; set; }
        public IRepository<T_StockExchange> StockExchanges { get; set; }
        public IRepository<M_Unit> Units { get; set; }
        public IRepository<M_Operation> WorkSchedules { get; set; }
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
