using System.Collections.Generic;
using Master40.DB.Data.WrappersForPrimitives;
using Master40.DB.DataModel;
using Zpp;
using Zpp.DemandDomain;
using Zpp.ProviderDomain;
using Zpp.Utils;

namespace Zpp
{
    public class ProductionOrderBomCreatorAssemblyLine : IProductionOrderBomCreator
    {
        private readonly Dictionary<M_Operation, ProductionOrderOperation>
            _alreadyCreatedProductionOrderOperations =
                new Dictionary<M_Operation, ProductionOrderOperation>();

        public ProductionOrderBomCreatorAssemblyLine()
        {
            if (Configuration.ProductionType.Equals(ProductionType.AssemblyLine) == false)
            {
                throw new MrpRunException("This is class is intended for productionType AssemblyLine.");
            }
        }

        public Demands CreateProductionOrderBomsForArticleBom(IDbMasterDataCache dbMasterDataCache,
            IDbTransactionData dbTransactionData, M_ArticleBom articleBom, Quantity quantity,
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
                    dbMasterDataCache.M_OperationGetById(
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
                    dbMasterDataCache, productionOrderOperation);

            if (newProductionOrderBom.HasOperation() == false)
            {
                throw new MrpRunException(
                    "Every PrOBom must have an operation. Add an operation to the articleBom.");
            }

            if (_alreadyCreatedProductionOrderOperations.ContainsKey(articleBom.Operation) == false)
            {
                _alreadyCreatedProductionOrderOperations.Add(articleBom.Operation,
                    newProductionOrderBom.GetProductionOrderOperation(dbTransactionData));
            }

            newProductionOrderBoms.Add(newProductionOrderBom);


            return newProductionOrderBoms;
        }
    }
}