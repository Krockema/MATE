using System.Collections.Generic;
using Master40.DB.Data.WrappersForPrimitives;
using Master40.DB.DataModel;
using Zpp.DataLayer;
using Zpp.DataLayer.impl.DemandDomain.Wrappers;
using Zpp.DataLayer.impl.DemandDomain.WrappersForCollections;
using Zpp.DataLayer.impl.ProviderDomain.Wrappers;
using Zpp.Util;

namespace Zpp.Mrp2.impl.Mrp1.impl.Production.impl.ProductionTypes
{
    /**
     * (articleBom.Quantity) ProductionOrderBoms will be created
     * ProductionOrderOperation.Duration == articleBom.Duration * quantity
     */
    public class ProductionOrderBomCreator
    : IProductionOrderBomCreator
    {
        private readonly Dictionary<M_Operation, ProductionOrderOperation>
            _alreadyCreatedProductionOrderOperations =
                new Dictionary<M_Operation, ProductionOrderOperation>();
        private readonly IDbMasterDataCache _dbMasterDataCache =
            ZppConfiguration.CacheManager.GetMasterDataCache();

        public ProductionOrderBomCreator()
        {

        }

        public Demands CreateProductionOrderBomsForArticleBom(
            M_ArticleBom articleBom, Quantity quantity,
            ProductionOrder parentProductionOrder)
        {
            
            Demands newProductionOrderBoms = new Demands();
            ProductionOrderOperation productionOrderOperation = null;
            if (articleBom.OperationId == null)
            {
                throw new MrpRunException(
                    "Every PrOBom must have an operation. Add an operation to the articleBom.");
            }

            // load articleBom.Operation
            if (articleBom.Operation == null)
            {
                articleBom.Operation =
                    _dbMasterDataCache.M_OperationGetById(
                        new Id(articleBom.OperationId.GetValueOrDefault()));
            }

            // don't create an productionOrderOperation twice, take existing
            if (_alreadyCreatedProductionOrderOperations.ContainsKey(articleBom.Operation))
            {

                    productionOrderOperation =
                        _alreadyCreatedProductionOrderOperations[articleBom.Operation];

            }

            ProductionOrderBom newProductionOrderBom =
                ProductionOrderBom.CreateTProductionOrderBom(articleBom, parentProductionOrder,
                    productionOrderOperation, quantity);

            if (newProductionOrderBom.HasOperation() == false)
            {
                throw new MrpRunException(
                    "Every PrOBom must have an operation. Add an operation to the articleBom.");
            }

            if (_alreadyCreatedProductionOrderOperations.ContainsKey(articleBom.Operation) == false)
            {
                _alreadyCreatedProductionOrderOperations.Add(articleBom.Operation,
                    newProductionOrderBom.GetProductionOrderOperation());
            }

            newProductionOrderBoms.Add(newProductionOrderBom);


            return newProductionOrderBoms;
        }
        
    }
}