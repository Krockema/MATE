using System.Collections.Generic;
using Master40.DB.DataModel;
using Master40.DB.Interfaces;
using Zpp.WrappersForPrimitives;

namespace Zpp.Wrappers
{
    /**
     * wraps T_ProductionOrder
     */
    public class ProductionOrder : Provider, IProviderLogic
    {
        public IProvider Provider => _provider;

        public ProductionOrder(IProvider provider, List<Demand> demands) : base(provider, demands)
        {
        }

        public ProductionOrder(M_Article article, DueTime dueTime, Quantity quantity, string productionOrderName)
        {
            T_ProductionOrder productionOrder = new T_ProductionOrder();
            // [ArticleId],[Quantity],[Name],[DueTime],[ProviderId]
            productionOrder.DueTime = dueTime.GetDueTime();
            productionOrder.Article = article;
            productionOrder.ArticleId = article.Id;
            productionOrder.Name = productionOrderName;
            // connects this provider with table T_Provider
            productionOrder.Provider = new T_Provider();
            productionOrder.Quantity = quantity.GetQuantity();
        }

        public override IProvider ToIProvider()
        {
            return (T_ProductionOrder)_provider;
        }
    }
}