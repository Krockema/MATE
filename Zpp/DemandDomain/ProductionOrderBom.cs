using Master40.DB.DataModel;
using Master40.DB.Enums;
using Master40.DB.Interfaces;

namespace Zpp.DemandDomain
{
    public class ProductionOrderBom : Demand, IDemandLogic
    {
        public ProductionOrderBom(IDemand demand) : base(demand)
        {
        }

       
        public ProductionOrderBom(M_ArticleBom articleBom,
            IProvider productionOrder) : base(CreateProductionOrderBom(articleBom,productionOrder))
        {
            
        }

        private static IDemand CreateProductionOrderBom(M_ArticleBom articleBom,
            IProvider productionOrder)
        {
            T_ProductionOrderBom productionOrderBom = new T_ProductionOrderBom();
            // TODO: Terminierung+Maschinenbelegung
            productionOrderBom.Quantity = articleBom.Quantity;
            productionOrderBom.State = State.Created;
            productionOrderBom.ProductionOrderParent = (T_ProductionOrder)productionOrder;
            productionOrderBom.ProductionOrderParentId = productionOrderBom.ProductionOrderParent.Id;
            productionOrderBom.ProductionOrderOperation =
                new T_ProductionOrderOperation(articleBom);
            productionOrderBom.ArticleChild = articleBom.ArticleChild;
            productionOrderBom.ArticleChildId = articleBom.ArticleChildId;

            return productionOrderBom;
        }

        public override IDemand ToIDemand()
        {
            return (T_ProductionOrderBom)_demand;
        }
    }
}