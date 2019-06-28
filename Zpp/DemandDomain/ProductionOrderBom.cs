using Master40.DB.Data.WrappersForPrimitives;
using Master40.DB.DataModel;
using Master40.DB.Enums;
using Master40.DB.Interfaces;
using Zpp.LotSize;
using Zpp.WrappersForPrimitives;

namespace Zpp.DemandDomain
{
    public class ProductionOrderBom : Demand, IDemandLogic
    {
        public ProductionOrderBom(IDemand demand, IDbMasterDataCache dbMasterDataCache) : base(
            demand, dbMasterDataCache)
        {
        }
        

        public static ProductionOrderBom CreateProductionOrderBom(M_ArticleBom articleBom,
            IProvider productionOrder, IDbMasterDataCache dbMasterDataCache,  ILotSize lotSize)
        {
            T_ProductionOrderBom productionOrderBom = new T_ProductionOrderBom();
            // TODO: Terminierung+Maschinenbelegung
            productionOrderBom.Quantity = articleBom.Quantity * lotSize.GetCalculatedQuantity().GetValue();
            productionOrderBom.State = State.Created;
            productionOrderBom.ProductionOrderParent = (T_ProductionOrder) productionOrder;
            // productionOrderBom.ProductionOrderParentId = productionOrderBom.ProductionOrderParent.Id;
            productionOrderBom.ProductionOrderOperation =
                new ProductionOrderOperation(articleBom).GetValue();
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

        public override DueTime GetDueTime()
        {
            DueTime dueTime =
                new DueTime(((T_ProductionOrderBom) _demand).ProductionOrderParent.DueTime);
            return dueTime;
        }
    }
}