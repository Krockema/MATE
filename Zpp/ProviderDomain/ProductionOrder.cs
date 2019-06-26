using System.Collections.Generic;
using Master40.DB.Data.WrappersForPrimitives;
using Master40.DB.DataModel;
using Master40.DB.Interfaces;
using Zpp.DemandDomain;
using ZppForPrimitives;

namespace Zpp.ProviderDomain
{
    /**
     * wraps T_ProductionOrder
     */
    public class ProductionOrder : Provider, IProviderLogic
    {
        public ProductionOrder(IProvider provider, Demands demands) : base(provider, demands)
        {
        }

        public ProductionOrder(IDemand demand, Demands childDemands) : base(CreateProductionOrder(demand), childDemands)
        {
        }

        private static IProvider CreateProductionOrder(IDemand demand)
        {
            T_ProductionOrder productionOrder = new T_ProductionOrder();
            // [ArticleId],[Quantity],[Name],[DueTime],[ProviderId]
            productionOrder.DueTime = demand.GetDueTime();
            productionOrder.Article = demand.GetArticle();
            productionOrder.ArticleId =  demand.GetArticle().Id;
            productionOrder.Name = $"ProductionOrder for Demand {demand.Id}";
            // connects this provider with table T_Provider
            productionOrder.Provider = new T_Provider();
            productionOrder.Quantity = demand.GetQuantity().GetValue();

            return productionOrder;
        }

        public override IProvider ToIProvider()
        {
            return (T_ProductionOrder) _provider;
        }
    }
}