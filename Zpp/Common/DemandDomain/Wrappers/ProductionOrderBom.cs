using System;
using Master40.DB.Data.WrappersForPrimitives;
using Master40.DB.DataModel;
using Master40.DB.Interfaces;
using Zpp.Common.ProviderDomain;
using Zpp.Common.ProviderDomain.Wrappers;
using Zpp.DbCache;
using Zpp.Mrp.ProductionManagement;
using Zpp.Utils;
using Zpp.WrappersForPrimitives;

namespace Zpp.Common.DemandDomain.Wrappers
{
    public class ProductionOrderBom : Demand, IDemandLogic
    {
        private readonly T_ProductionOrderBom _productionOrderBom;

        public ProductionOrderBom(IDemand demand, IDbMasterDataCache dbMasterDataCache) : base(
            demand, dbMasterDataCache)
        {
            _productionOrderBom = (T_ProductionOrderBom) _demand;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="articleBom"></param>
        /// <param name="parentProductionOrder"></param>
        /// <param name="dbMasterDataCache"></param>
        /// <param name="quantity">of production article to produce
        /// --> is used for childs as: articleBom.Quantity * quantity</param>
        /// <param name="productionOrderOperation">use already created, null if no one was created before</param>
        /// <returns></returns>
        public static ProductionOrderBom CreateTProductionOrderBom(M_ArticleBom articleBom,
            Provider parentProductionOrder, IDbMasterDataCache dbMasterDataCache,
            ProductionOrderOperation productionOrderOperation, Quantity quantity)
        {
            T_ProductionOrderBom productionOrderBom = new T_ProductionOrderBom();
            // TODO: Terminierung+Maschinenbelegung
            productionOrderBom.Quantity = articleBom.Quantity * quantity.GetValue();
            productionOrderBom.ProductionOrderParent =
                (T_ProductionOrder) parentProductionOrder.ToIProvider();
            productionOrderBom.ProductionOrderParentId =
                productionOrderBom.ProductionOrderParent.Id;

            // bom is toPurchase if articleBom.Operation == null
            if (productionOrderOperation != null)
            {
                productionOrderBom.ProductionOrderOperation = productionOrderOperation.GetValue();
                productionOrderBom.ProductionOrderOperationId =
                    productionOrderBom.ProductionOrderOperation.Id;
            }

            if (productionOrderOperation == null && articleBom.Operation != null)
            {
                productionOrderBom.ProductionOrderOperation =
                    ProductionManager.CreateProductionOrderOperation(articleBom,
                        parentProductionOrder, quantity);
                productionOrderBom.ProductionOrderOperationId =
                    productionOrderBom.ProductionOrderOperation.Id;
            }


            productionOrderBom.ArticleChild = articleBom.ArticleChild;
            productionOrderBom.ArticleChildId = articleBom.ArticleChildId;

            return new ProductionOrderBom(productionOrderBom, dbMasterDataCache);
        }

        public override IDemand ToIDemand()
        {
            return _productionOrderBom;
        }

        public override M_Article GetArticle()
        {
            Id articleId = new Id(_productionOrderBom.ArticleChildId);
            return _dbMasterDataCache.M_ArticleGetById(articleId);
        }

        /**
         * @return:
         *   if ProductionOrderOperation is backwardsScheduled --> EndBackward
         *   else ProductionOrderParent.dueTime
         */
        public override DueTime GetDueTime(IDbTransactionData dbTransactionData)
        {
            // load ProductionOrderOperation if not done yet
            if (_productionOrderBom.ProductionOrderOperation == null)
            {
                Id productionOrderOperationId =
                    new Id(_productionOrderBom.ProductionOrderOperationId.GetValueOrDefault());
                _productionOrderBom.ProductionOrderOperation = dbTransactionData
                    .ProductionOrderOperationGetById(productionOrderOperationId);
            }


            DueTime dueTime;
            if (_productionOrderBom.ProductionOrderOperation != null &&
                _productionOrderBom.ProductionOrderOperation.EndBackward != null)
                // backwards scheduling was already done --> job-shop-scheduling was done --> return End
            {
                if (GetArticle().ToBuild)
                {
                    dueTime = new DueTime(_productionOrderBom.ProductionOrderOperation.EndBackward
                        .GetValueOrDefault());
                    return dueTime;
                }
                else
                {
                    dueTime = new DueTime(_productionOrderBom.ProductionOrderOperation.StartBackward
                        .GetValueOrDefault());
                    return dueTime;
                }
            }
            else
            {
                throw new MrpRunException(
                    "Requesting dueTime for ProductionOrderBom before it was backwards-scheduled.");
            }


            /*
             // backwards scheduling was not yet done --> return dueTime of ProductionOrderParent
             if (_productionOrderBom.ProductionOrderParent == null)
            {
                Id productionOrderId = new Id(_productionOrderBom.ProductionOrderParentId);
                _productionOrderBom.ProductionOrderParent = (T_ProductionOrder) dbTransactionData
                    .ProvidersGetById(productionOrderId).ToIProvider();
            }

            dueTime = new DueTime(_productionOrderBom.ProductionOrderParent.DueTime);
            return dueTime;*/
        }

        public override string GetGraphizString(IDbTransactionData dbTransactionData)
        {
            // Demand(CustomerOrder);20;Truck

            string graphizString;
            if (_productionOrderBom.ProductionOrderOperationId != null)
            {
                if (_productionOrderBom.ProductionOrderOperation == null)
                {
                    _productionOrderBom.ProductionOrderOperation =
                        dbTransactionData.ProductionOrderOperationGetById(new Id(_productionOrderBom
                            .ProductionOrderOperationId.GetValueOrDefault()));
                }

                T_ProductionOrderOperation tProductionOrderOperation =
                    _productionOrderBom.ProductionOrderOperation;
                graphizString = $"D(PrOB);{base.GetGraphizString(dbTransactionData)};" +
                                $"bs({tProductionOrderOperation.StartBackward});" +
                                $"be({tProductionOrderOperation.EndBackward});\\n{tProductionOrderOperation}";
            }
            else
            {
                graphizString = $"D(PrOB);{base.GetGraphizString(dbTransactionData)}";
            }

            return graphizString;
        }

        public bool HasOperation()
        {
            return _productionOrderBom.ProductionOrderOperationId != null;
        }

        public ProductionOrderOperation GetProductionOrderOperation(
            IDbTransactionData dbTransactionData)
        {
            if (_productionOrderBom.ProductionOrderOperationId == null)
            {
                return null;
            }

            if (_productionOrderBom.ProductionOrderOperation == null)
                // load it
            {
                _productionOrderBom.ProductionOrderOperation =
                    dbTransactionData.ProductionOrderOperationGetById(
                        new Id(_productionOrderBom.ProductionOrderOperationId.GetValueOrDefault()));
            }

            return new ProductionOrderOperation(_productionOrderBom.ProductionOrderOperation,
                _dbMasterDataCache);
        }

        public override DueTime GetStartTime(IDbTransactionData dbTransactionData)
        {
            if (_productionOrderBom.ProductionOrderOperationId != null)
            {
                if (_productionOrderBom.ProductionOrderOperation == null)
                // load it
                {
                    _productionOrderBom.ProductionOrderOperation =
                        dbTransactionData.ProductionOrderOperationGetById(new Id(_productionOrderBom
                            .ProductionOrderOperationId.GetValueOrDefault()));
                }

                T_ProductionOrderOperation tProductionOrderOperation =
                    _productionOrderBom.ProductionOrderOperation;
                if (tProductionOrderOperation.StartBackward == null)
                {
                    throw new MrpRunException("Requesting start time of ProductionOrderBom before it was backwards-scheduled.");
                }
                return new DueTime(tProductionOrderOperation.StartBackward.GetValueOrDefault());
            }
            else
            {
                return null;
            }
        }

        public ProductionOrder GetProductionOrder(IDbTransactionData dbTransactionData)
        {
            if (_productionOrderBom.ProductionOrderParent == null)
            {
                var productionOrder = dbTransactionData.ProductionOrderGetById(new Id(_productionOrderBom.ProductionOrderParentId))
                                                       .ToIProvider() as T_ProductionOrder;
                if (productionOrder == null)
                {
                    throw new Exception("ProductionOrderBom must have one ProductionOrderParent");
                }

                _productionOrderBom.ProductionOrderParent = productionOrder;
            }


            return new ProductionOrder(_productionOrderBom.ProductionOrderParent,
                _dbMasterDataCache);
        }

        public M_ArticleBom GetArticleBom()
        {
            return _dbMasterDataCache.M_ArticleBomGetByArticleChildId(
                new Id(_productionOrderBom.ArticleChildId));
        }
    }
}