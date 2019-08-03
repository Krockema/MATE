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
            productionOrderBom.ProductionOrderParent =
                (T_ProductionOrder) parentProductionOrder.ToIProvider();
            productionOrderBom.ProductionOrderParentId =
                productionOrderBom.ProductionOrderParent.Id;
            // bom is toPurchase if it's null
            if (articleBom.Operation != null)
            {
                productionOrderBom.ProductionOrderOperation =
                    ProductionOrderOperation.CreateProductionOrderOperation(articleBom,
                        parentProductionOrder);
                productionOrderBom.ProductionOrderOperationId =
                    productionOrderBom.ProductionOrderOperation.Id;
            }

            productionOrderBom.ArticleChild = articleBom.ArticleChild;
            productionOrderBom.ArticleChildId = articleBom.ArticleChildId;

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

        public override DueTime GetDueTime(IDbTransactionData dbTransactionData)
        {
            T_ProductionOrderBom productionOrderBom = ((T_ProductionOrderBom) _demand);
            if (productionOrderBom.ProductionOrderParent == null)
            {
                Id productionOrderId = new Id(productionOrderBom.ProductionOrderParentId);
                productionOrderBom.ProductionOrderParent = (T_ProductionOrder) dbTransactionData
                    .ProvidersGetById(productionOrderId).ToIProvider();
            }

            DueTime dueTime = new DueTime(productionOrderBom.ProductionOrderParent.DueTime);
            return dueTime;
        }

        public override string GetGraphizString(IDbTransactionData dbTransactionData)
        {
            // Demand(CustomerOrder);20;Truck
            string graphizString = $"D(PrOB);{base.GetGraphizString(dbTransactionData)}";
            return graphizString;
        }
    }
}