using Master40.DB.DataModel;
using Master40.DB.Enums;
using Master40.DB.Interfaces;

namespace Zpp.Wrappers
{
    public class ProductionOrderBom : Demand, IDemandLogic
    {
        private T_ProductionOrderBom _productionOrderBom;
        public ProductionOrderBom(IDemand demand) : base(demand)
        {
        }

       
        public ProductionOrderBom(M_ArticleBom articleBom,
            IProvider productionOrder)
        {
            _productionOrderBom = new T_ProductionOrderBom();
            // TODO: Terminierung+Maschinenbelegung
            _productionOrderBom.Quantity = articleBom.Quantity;
            _productionOrderBom.State = State.Created;
            _productionOrderBom.ProductionOrderParent = (T_ProductionOrder)productionOrder;
            _productionOrderBom.ProductionOrderParentId = _productionOrderBom.ProductionOrderParent.Id;
            _productionOrderBom.ProductionOrderOperation =
                new T_ProductionOrderOperation(articleBom);
            _productionOrderBom.ArticleChild = articleBom.ArticleChild;
            _productionOrderBom.ArticleChildId = articleBom.ArticleChildId;
        }

        public override IDemand ToIDemand()
        {
            return (T_ProductionOrderBom)_demand;
        }
    }
}