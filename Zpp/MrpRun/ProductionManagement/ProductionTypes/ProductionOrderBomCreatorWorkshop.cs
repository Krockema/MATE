using System.Collections.Generic;
using Master40.DB.Data.WrappersForPrimitives;
using Master40.DB.DataModel;
using Zpp.DemandDomain;
using Zpp.Utils;

namespace Zpp.ProviderDomain
{
    /**
     * This class exists because of following problem:
     * - Lotsize must not be equal to the articleBom.Quantity
     * --> multiple ProdcutionOrderBoms need to be created, because every operation consume only
     * quantity of articleBom.Quantity
     */
    public class ProductionOrderBomCreatorWorkshop : IProductionOrderBomCreator
    {
        private readonly Dictionary<M_Operation, List<ProductionOrderOperation>>
            _alreadyCreatedProductionOrderOperations =
                new Dictionary<M_Operation, List<ProductionOrderOperation>>();

        public ProductionOrderBomCreatorWorkshop()
        {
            if (Configuration.ProductionType.Equals(ProductionType.WorkshopProduction) == false)
            {
                throw new MrpRunException("This is class is intended for productionType WorkshopProduction.");
            }
        }

        public Demands CreateProductionOrderBomsForArticleBom(IDbMasterDataCache dbMasterDataCache,
            IDbTransactionData dbTransactionData, M_ArticleBom articleBom, Quantity quantity,
            ProductionOrder parentProductionOrder)
        {

            Demands newProductionOrderBoms = new Demands();
            for (int i = 0; i < quantity.GetValue(); i++)
            {
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
                    if (_alreadyCreatedProductionOrderOperations[articleBom.Operation].Count > i)
                    {
                        productionOrderOperation =
                            _alreadyCreatedProductionOrderOperations[articleBom.Operation][i];
                    }
                }
                else
                {
                    _alreadyCreatedProductionOrderOperations.Add(articleBom.Operation,
                        new List<ProductionOrderOperation>());
                }

                ProductionOrderBom newProductionOrderBom =
                    ProductionOrderBom.CreateTProductionOrderBom(articleBom, parentProductionOrder,
                        dbMasterDataCache, productionOrderOperation);

                if (newProductionOrderBom.HasOperation() == false)
                {
                    throw new MrpRunException(
                        "Every PrOBom must have an operation. Add an operation to the articleBom.");
                }
                
                if (_alreadyCreatedProductionOrderOperations[articleBom.Operation].Count <= i)
                {
                    _alreadyCreatedProductionOrderOperations[articleBom.Operation].Add(
                        newProductionOrderBom.GetProductionOrderOperation(dbTransactionData));
                }
                
                newProductionOrderBoms.Add(newProductionOrderBom);
                
            }

            return newProductionOrderBoms;
        }
    }
}