using System.Collections.Generic;
using Master40.DB.DataModel;

namespace Master40.DB.Data.Context
{
    public class SimulationDbState
    {
        public List<Article> Articles { get; set; }
        public List<ArticleBom> ArticleBoms { get; set; }
        public List<ArticleType> ArticleTypes { get; set; }
        public List<ArticleToBusinessPartner> ArticleToBusinessPartners { get; set; }
        public List<BusinessPartner> BusinessPartners { get; set; }
        public List<DemandToProvider> Demands { get; set; }
        //public IQueryable<DemandToProvider> DemandToProvider { get; set; }
        public List<Machine> Machines { get; set; }
        public List<MachineGroup> MachineGroups { get; set; }
        public List<MachineTool> MachineTools { get; set; }
        public List<WorkSchedule> WorkSchedules { get; set; }
        public List<Order> Orders { get; set; }
        public List<OrderPart> OrderParts { get; set; }
        public List<Purchase> Purchases { get; set; }
        public List<ProductionOrder> ProductionOrders { get; set; }
        public List<ProductionOrderBom> ProductionOrderBoms { get; set; }
        public List<ProductionOrderWorkSchedule> ProductionOrderWorkSchedule { get; set; }
        public List<PurchasePart> PurchaseParts { get; set; }
        public List<Stock> Stocks { get; set; }
        public List<StockExchange> StockExchanges { get; set; }
        public List<Unit> Units { get; set; }
        public List<Kpi> Kpi { get; set; }
        public List<SimulationConfiguration> SimulationConfigurations { get; set; }
    }
}