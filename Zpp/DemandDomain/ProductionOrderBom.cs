using Master40.DB.Data.WrappersForPrimitives;
using Master40.DB.DataModel;
using Master40.DB.Enums;
using Master40.DB.Interfaces;
using Zpp.LotSize;
using Zpp.ProviderDomain;
using Zpp.WrappersForPrimitives;

namespace Zpp.DemandDomain
{
    public class ProductionOrderBom : Demand, IDemandLogic
    {
        public ProductionOrderBom(IDemand demand, IDbMasterDataCache dbMasterDataCache) : base(
            demand, dbMasterDataCache)
        {
        }
        

        /// <summary>
        /// 
        /// </summary>
        /// <param name="articleBom"></param>
        /// <param name="parentProductionOrder"></param>
        /// <param name="dbMasterDataCache"></param>
        /// <param name="quantity">of production article to produce
        /// --> is used for childs as: articleBom.Quantity * quantity</param>
        /// <returns></returns>
        public static ProductionOrderBom CreateProductionOrderBom(M_ArticleBom articleBom,
            Provider parentProductionOrder, IDbMasterDataCache dbMasterDataCache, Quantity quantity)
        {
            T_ProductionOrderBom productionOrderBom = new T_ProductionOrderBom();
            // TODO: Terminierung+Maschinenbelegung
            productionOrderBom.Quantity = articleBom.Quantity * quantity.GetValue();
            productionOrderBom.State = State.Created;
            productionOrderBom.ProductionOrderParent = (T_ProductionOrder) parentProductionOrder.ToIProvider();
            productionOrderBom.ProductionOrderParentId = productionOrderBom.ProductionOrderParent.Id;
            productionOrderBom.ProductionOrderOperation =
                ProductionOrderOperation.CreateProductionOrderOperation(articleBom, parentProductionOrder);
            productionOrderBom.ProductionOrderOperationId =
                productionOrderBom.ProductionOrderOperation.Id;
            productionOrderBom.ArticleChild = articleBom.ArticleChild;
            productionOrderBom.ArticleChildId = articleBom.ArticleChildId;
            productionOrderBom.Demand = new T_Demand();
            productionOrderBom.DemandId = productionOrderBom.Demand.Id;

            return new ProductionOrderBom(productionOrderBom, dbMasterDataCache);
        }

        public override IDemand ToIDemand()
        {
            return (T_ProductionOrderBom) _demand;
        }

        public override M_Article GetArticle()
        {
            Id articleId = new Id(((T_ProductionOrderBom) _demand).ArticleChildId);
            return _dbMasterDataCache.M_ArticleGetById(articleId);
        }

        public override DueTime GetDueTime()
        {
            DueTime dueTime =
                new DueTime(((T_ProductionOrderBom) _demand).ProductionOrderParent.DueTime);
            return dueTime;
        }
    }
}