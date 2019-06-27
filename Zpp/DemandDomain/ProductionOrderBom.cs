using Master40.DB.Data.WrappersForPrimitives;
using Master40.DB.DataModel;
using Master40.DB.Enums;
using Master40.DB.Interfaces;
using Zpp.WrappersForPrimitives;

namespace Zpp.DemandDomain
{
    public class ProductionOrderBom : Demand, IDemandLogic
    {
        public ProductionOrderBom(IDemand demand, IDbCacheMasterData dbCacheMasterData) : base(
            demand, dbCacheMasterData)
        {
        }


        public ProductionOrderBom(M_ArticleBom articleBom, IProvider productionOrder,
            IDbCacheMasterData dbCacheMasterData) : base(
            CreateProductionOrderBom(articleBom, productionOrder), dbCacheMasterData)
        {
        }

        private static IDemand CreateProductionOrderBom(M_ArticleBom articleBom,
            IProvider productionOrder)
        {
            T_ProductionOrderBom productionOrderBom = new T_ProductionOrderBom();
            // TODO: Terminierung+Maschinenbelegung
            productionOrderBom.Quantity = articleBom.Quantity;
            productionOrderBom.State = State.Created;
            productionOrderBom.ProductionOrderParent = (T_ProductionOrder) productionOrder;
            productionOrderBom.ProductionOrderParentId =
                productionOrderBom.ProductionOrderParent.Id;
            productionOrderBom.ProductionOrderOperation =
                new ProductionOrderOperation(articleBom).GetValue();
            productionOrderBom.ArticleChild = articleBom.ArticleChild;
            productionOrderBom.ArticleChildId = articleBom.ArticleChildId;

            return productionOrderBom;
        }

        public override IDemand ToIDemand()
        {
            return (T_ProductionOrderBom) _demand;
        }

        public override M_Article GetArticle()
        {
            Id articleId = new Id(((T_ProductionOrderBom) _demand).ArticleChildId);
            return _dbCacheMasterData.M_ArticleGetById(articleId);
        }

        public override DueTime GetDueTime()
        {
            DueTime dueTime =
                new DueTime(((T_ProductionOrderBom) _demand).ProductionOrderParent.DueTime);
            return dueTime;
        }
    }
}