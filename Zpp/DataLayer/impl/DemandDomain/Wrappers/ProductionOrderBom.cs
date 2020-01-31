using System;
using Master40.DB.Data.WrappersForPrimitives;
using Master40.DB.DataModel;
using Master40.DB.Nominal;
using Master40.DB.Interfaces;
using Zpp.DataLayer.impl.ProviderDomain;
using Zpp.DataLayer.impl.ProviderDomain.Wrappers;
using Zpp.Mrp2.impl.Mrp1.impl.Production.impl;
using Zpp.Mrp2.impl.Scheduling.impl;
using Zpp.Util;

namespace Zpp.DataLayer.impl.DemandDomain.Wrappers
{
    public class ProductionOrderBom : Demand, IDemandLogic
    {
        private readonly T_ProductionOrderBom _productionOrderBom;

        public ProductionOrderBom(IDemand demand) : base(demand)
        {
            _productionOrderBom = (T_ProductionOrderBom) _demand;
            // EnsureOperationIsLoadedIfExists();
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
            Provider parentProductionOrder, ProductionOrderOperation productionOrderOperation,
            Quantity quantity)
        {
            T_ProductionOrderBom productionOrderBom = new T_ProductionOrderBom();
            if (quantity == null || quantity.GetValue() == 0)
            {
                throw new MrpRunException("Quantity is not set.");
            }

            productionOrderBom.Quantity = articleBom.Quantity * quantity.GetValue(); // TODO: PASCAL .GetValueOrDefault();
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

            return new ProductionOrderBom(productionOrderBom);
        }

        public override IDemand ToIDemand()
        {
            return _productionOrderBom;
        }

        public override M_Article GetArticle()
        {
            return _dbMasterDataCache.M_ArticleGetById(GetArticleId());
        }

        public override Id GetArticleId()
        {
            return new Id(_productionOrderBom.ArticleChildId);
        }

        public bool HasOperation()
        {
            return _productionOrderBom.ProductionOrderOperationId != null;
        }

        public Id GetProductionOrderOperationId()
        {
            if (_productionOrderBom.ProductionOrderOperationId == null)
            {
                return null;
            }
            return new Id(_productionOrderBom.ProductionOrderOperationId.GetValueOrDefault());
        }

        public ProductionOrderOperation GetProductionOrderOperation()
        {
            if (_productionOrderBom.ProductionOrderOperationId == null)
            {
                return null;
            }

            EnsureOperationIsLoadedIfExists();

            return new ProductionOrderOperation(_productionOrderBom.ProductionOrderOperation);
        }

        public DueTime GetStartTimeOfOperation()
        {
            EnsureOperationIsLoadedIfExists();

            if (_productionOrderBom.ProductionOrderOperation?.StartBackward != null)
                // backwards scheduling was already done --> job-shop-scheduling was done
            {
                T_ProductionOrderOperation productionOrderOperation =
                    _productionOrderBom.ProductionOrderOperation;
                DueTime dueTime =
                    new DueTime(productionOrderOperation.StartBackward.GetValueOrDefault());
                return dueTime;
            }
            else
            {
                throw new MrpRunException(
                    "Requesting dueTime for ProductionOrderBom before it was backwards-scheduled.");
            }
        }

        public DueTime GetEndTimeOfOperation()
        {
            EnsureOperationIsLoadedIfExists();

            if (_productionOrderBom.ProductionOrderOperation?.EndBackward != null)
                // backwards scheduling was already done --> job-shop-scheduling was done
            {
                T_ProductionOrderOperation productionOrderOperation =
                    _productionOrderBom.ProductionOrderOperation;
                DueTime dueTime =
                    new DueTime(productionOrderOperation.EndBackward.GetValueOrDefault());
                return dueTime;
            }
            else
            {
                throw new MrpRunException(
                    "Requesting dueTime for ProductionOrderBom before it was backwards-scheduled.");
            }
        }

        public void EnsureOperationIsLoadedIfExists()
        {
            EnsureOperationIsLoadedIfExists(ZppConfiguration.CacheManager.GetDbTransactionData());
        }

        private void EnsureOperationIsLoadedIfExists(IDbTransactionData dbTransactionData)
        {
            // load ProductionOrderOperation if not done yet
            if (_productionOrderBom.ProductionOrderOperation == null)
            {
                Id productionOrderOperationId =
                    new Id(_productionOrderBom.ProductionOrderOperationId.GetValueOrDefault());

                _productionOrderBom.ProductionOrderOperation = dbTransactionData
                    .ProductionOrderOperationGetById(productionOrderOperationId).GetValue();
            }
        }

        public ProductionOrder GetProductionOrder()
        {
            IDbTransactionData dbTransactionData =
                ZppConfiguration.CacheManager.GetDbTransactionData();
            if (_productionOrderBom.ProductionOrderParent == null)
            {
                var productionOrder =
                    dbTransactionData
                        .ProductionOrderGetById(new Id(_productionOrderBom.ProductionOrderParentId))
                        .ToIProvider() as T_ProductionOrder;
                if (productionOrder == null)
                {
                    throw new Exception("ProductionOrderBom must have one ProductionOrderParent");
                }

                _productionOrderBom.ProductionOrderParent = productionOrder;
            }


            return new ProductionOrder(_productionOrderBom.ProductionOrderParent);
        }

        public M_ArticleBom GetArticleBom()
        {
            return _dbMasterDataCache.M_ArticleBomGetByArticleChildId(
                new Id(_productionOrderBom.ArticleChildId));
        }

        public override Duration GetDuration()
        {
            EnsureOperationIsLoadedIfExists();
            Duration operationDuration = GetDurationOfOperation();
            Duration transitionTime =
                new Duration(TransitionTimer.CalculateTransitionTime(operationDuration));
            return transitionTime.Plus(operationDuration);
        }

        public Duration GetDurationOfOperation()
        {
            EnsureOperationIsLoadedIfExists();
            return _productionOrderBom.ProductionOrderOperation.GetDuration();
        }

        public override void SetStartTimeBackward(DueTime startTime)
        {
            throw new NotImplementedException();
        }

        public override void SetFinished()
        {
            EnsureOperationIsLoadedIfExists();
            _productionOrderBom.ProductionOrderOperation.State = State.Finished;
        }

        public override void SetInProgress()
        {
            EnsureOperationIsLoadedIfExists();
            if (_productionOrderBom.ProductionOrderOperation.State.Equals(State.Finished))
            {
                throw new MrpRunException("Impossible, the operation is already finished.");
            }

            _productionOrderBom.ProductionOrderOperation.State = State.InProgress;
        }

        public override DueTime GetEndTimeBackward()
        {
            return GetEndTimeOfOperation();
        }

        public override bool IsFinished()
        {
            EnsureOperationIsLoadedIfExists();
            return _productionOrderBom.ProductionOrderOperation.State.Equals(State.Finished);
        }

        public override void SetEndTimeBackward(DueTime endTime)
        {
            throw new NotImplementedException();
        }

        public override void ClearStartTimeBackward()
        {
            throw new NotImplementedException();
        }

        public override void ClearEndTimeBackward()
        {
            throw new NotImplementedException();
        }

        public override State? GetState()
        {
                EnsureOperationIsLoadedIfExists();
                return _productionOrderBom.ProductionOrderOperation.State;

        }
    }
}