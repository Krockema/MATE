using System.Collections.Generic;
using Master40.DB.DataModel;

namespace Master40.DB.Data.Context
{
    public class SimulationDbState
    {
        public List<M_Article> Articles { get; set; }
        public List<M_ArticleBom> ArticleBoms { get; set; }
        public List<M_ArticleType> ArticleTypes { get; set; }
        public List<M_ArticleToBusinessPartner> ArticleToBusinessPartners { get; set; }
        public List<M_BusinessPartner> BusinessPartners { get; set; }
        public List<T_DemandToProvider> Demands { get; set; }
        //public IQueryable<DemandToProvider> DemandToProvider { get; set; }
        public List<M_Machine> Machines { get; set; }
        public List<M_MachineGroup> MachineGroups { get; set; }
        public List<M_MachineTool> MachineTools { get; set; }
        public List<M_Operation> WorkSchedules { get; set; }
        public List<T_CustomerOrder> Orders { get; set; }
        public List<T_CustomerOrderPart> OrderParts { get; set; }
        public List<T_PurchaseOrder> Purchases { get; set; }
        public List<T_ProductionOrder> ProductionOrders { get; set; }
        public List<T_ProductionOrderBom> ProductionOrderBoms { get; set; }
        public List<T_ProductionOrderOperation> ProductionOrderWorkSchedule { get; set; }
        public List<T_PurchaseOrderPart> PurchaseParts { get; set; }
        public List<M_Stock> Stocks { get; set; }
        public List<T_StockExchange> StockExchanges { get; set; }
        public List<M_Unit> Units { get; set; }
    }
}