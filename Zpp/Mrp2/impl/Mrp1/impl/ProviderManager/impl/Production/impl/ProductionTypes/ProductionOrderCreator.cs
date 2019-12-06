using Master40.DB.Data.WrappersForPrimitives;
using Master40.DB.DataModel;
using Zpp.DataLayer.impl.DemandDomain;
using Zpp.DataLayer.impl.ProviderDomain.Wrappers;
using Zpp.DataLayer.impl.WrapperForEntities;
using Zpp.Util;

namespace Zpp.Mrp2.impl.Mrp1.impl.Production.impl.ProductionTypes
{
    /**
     * Here one ProductionOrder with productionOrder.Quantity == given quantity will be created
     */
    public class ProductionOrderCreator : IProductionOrderCreator
    {
        public ProductionOrderCreator()
        {

        }

        public EntityCollector CreateProductionOrder(Demand demand, Quantity quantity)
        {
            if (quantity == null || quantity.GetValue() == null)
            {
                throw new MrpRunException("Quantity is not set.");
            }
            T_ProductionOrder tProductionOrder = new T_ProductionOrder();
            // [ArticleId],[Quantity],[Name],[DueTime],[ProviderId]
            tProductionOrder.DueTime = demand.GetStartTimeBackward().GetValue();
            tProductionOrder.Article = demand.GetArticle();
            tProductionOrder.ArticleId = demand.GetArticle().Id;
            tProductionOrder.Name = $"ProductionOrder for Demand {demand.GetArticle()}";
            tProductionOrder.Quantity = quantity.GetValue(); // TODO: PASCAL .GetValueOrDefault();

            ProductionOrder productionOrder =
                new ProductionOrder(tProductionOrder);

            EntityCollector entityCollector = new EntityCollector();
            entityCollector.Add(productionOrder);
            
            return entityCollector;
        }
    }
}