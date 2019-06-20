using Master40.DB.DataModel;
using Master40.DB.Enums;
using Master40.DB.Interfaces;

namespace Zpp.Wrappers
{
    public class WT_ProductionOrderBom : Demand, WIDemand
    {

        public WT_ProductionOrderBom(IDemand demand) : base(demand)
        {
        }

        public WT_ProductionOrderBom(M_ArticleBom articleBom,  WT_ProductionOrder productionOrder)
        {
            _demand = CreateProductionOrderBom(articleBom, productionOrder);
        }
        
        private T_ProductionOrderBom CreateProductionOrderBom(M_ArticleBom articleBom,
            WT_ProductionOrder productionOrder)
        {
            T_ProductionOrderBom productionOrderBom = new T_ProductionOrderBom();
            
            // TODO: Terminierung+Maschinenbelegung
            productionOrderBom.Quantity = articleBom.Quantity;
            productionOrderBom.State = State.Created;
            productionOrderBom.ProductionOrderParent = (T_ProductionOrder)productionOrder.Provider;
            productionOrderBom.ProductionOrderParentId = productionOrderBom.ProductionOrderParent.Id;
            productionOrderBom.ProductionOrderOperation =
                CreateProductionOrderBomOperation(articleBom);
            productionOrderBom.ArticleChild = articleBom.ArticleChild;
            productionOrderBom.ArticleChildId = articleBom.ArticleChildId;
            return productionOrderBom;
        }
    }
}