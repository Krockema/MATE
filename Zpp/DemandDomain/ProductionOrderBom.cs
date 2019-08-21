using Master40.DB.Data.WrappersForPrimitives;
using Master40.DB.DataModel;
using Master40.DB.Enums;
using Master40.DB.Interfaces;
using Zpp.LotSize;
using Zpp.ProviderDomain;
using Zpp.SchedulingDomain;
using Zpp.Utils;
using Zpp.WrappersForPrimitives;

namespace Zpp.DemandDomain
{
    public class ProductionOrderBom : Demand, IDemandLogic
    {
        private  readonly T_ProductionOrderBom _productionOrderBom;
        
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
        public static ProductionOrderBom CreateProductionOrderBom(M_ArticleBom articleBom,
            Provider parentProductionOrder, IDbMasterDataCache dbMasterDataCache, Quantity quantity,
            ProductionOrderOperation productionOrderOperation)
        {
            T_ProductionOrderBom productionOrderBom = new T_ProductionOrderBom();
            // TODO: Terminierung+Maschinenbelegung
            productionOrderBom.Quantity = articleBom.Quantity * quantity.GetValue();
            productionOrderBom.State = State.Created;
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
                    ProductionOrderOperation.CreateProductionOrderOperation(articleBom,
                        parentProductionOrder);
                productionOrderBom.ProductionOrderOperationId =
                    productionOrderBom.ProductionOrderOperation.Id;
            }


            productionOrderBom.ArticleChild = articleBom.ArticleChild;
            productionOrderBom.ArticleChildId = articleBom.ArticleChildId;

            return new ProductionOrderBom(productionOrderBom, dbMasterDataCache);
        }

        public override IDemand GetIDemand()
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
            {
                // backwards scheduling was already done --> job-shop-scheduling was done --> return End
                dueTime = new DueTime(_productionOrderBom.ProductionOrderOperation.End);
                return dueTime;
            }
            // backwards scheduling was not yet done --> return dueTime of ProductionOrderParent

            if (_productionOrderBom.ProductionOrderParent == null)
            {
                Id productionOrderId = new Id(_productionOrderBom.ProductionOrderParentId);
                _productionOrderBom.ProductionOrderParent = (T_ProductionOrder) dbTransactionData
                    .ProvidersGetById(productionOrderId).ToIProvider();
            }

            dueTime = new DueTime(_productionOrderBom.ProductionOrderParent.DueTime);
            return dueTime;
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

        public OperationBackwardsSchedule ScheduleBackwards(
            OperationBackwardsSchedule lastOperationBackwardsSchedule)
        {
            DueTime TIME_BETWEEN_OPERATIONS =
                new DueTime(_productionOrderBom.ProductionOrderOperation.Duration * 3);
            int? startBackwards;
            int? endBackwards;
            // case: equal hierarchyNumber --> PrOO runs in parallel
            if (lastOperationBackwardsSchedule.GetHierarchyNumber() == null ||
                (lastOperationBackwardsSchedule.GetHierarchyNumber() != null &&
                 _productionOrderBom.ProductionOrderOperation.HierarchyNumber.Equals(
                     lastOperationBackwardsSchedule.GetHierarchyNumber().GetValue())))
            {
                endBackwards = lastOperationBackwardsSchedule.GetEndBackwards().GetValue();
                startBackwards = lastOperationBackwardsSchedule.GetEndBackwards().GetValue() -
                                 _productionOrderBom.ProductionOrderOperation.Duration;
            }
            // case: greaterHierarchyNumber --> PrOO runs after the last PrOO
            else
            {
                if (lastOperationBackwardsSchedule.GetHierarchyNumber().GetValue() <
                    _productionOrderBom.ProductionOrderOperation.HierarchyNumber)
                {
                    throw new MrpRunException(
                        "This is not allowed: hierarchyNumber of lastBackwardsSchedule " +
                        "is smaller than hierarchyNumber of current PrOO.");
                }

                endBackwards = lastOperationBackwardsSchedule.GetStartBackwards().GetValue();
                startBackwards = lastOperationBackwardsSchedule.GetStartBackwards().GetValue() -
                                 _productionOrderBom.ProductionOrderOperation.Duration;
            }

            _productionOrderBom.ProductionOrderOperation.EndBackward = endBackwards;
            _productionOrderBom.ProductionOrderOperation.StartBackward = startBackwards;

            // create return value
            OperationBackwardsSchedule newOperationBackwardsSchedule =
                new OperationBackwardsSchedule(new DueTime(startBackwards.GetValueOrDefault()),
                    new DueTime(endBackwards.GetValueOrDefault() -
                                TIME_BETWEEN_OPERATIONS.GetValue()),
                    new HierarchyNumber(
                        _productionOrderBom.ProductionOrderOperation.HierarchyNumber));

            return newOperationBackwardsSchedule;
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
                {
                    _productionOrderBom.ProductionOrderOperation =
                        dbTransactionData.ProductionOrderOperationGetById(new Id(_productionOrderBom
                            .ProductionOrderOperationId.GetValueOrDefault()));
                }

                T_ProductionOrderOperation tProductionOrderOperation =
                    _productionOrderBom.ProductionOrderOperation;
                return new DueTime(tProductionOrderOperation.Start);
            }
            else
            {
                return null;
            }
        }

        public ProductionOrder GetProductionOrder()
        {
            return new ProductionOrder(_productionOrderBom.ProductionOrderParent, _dbMasterDataCache);
        }

        public M_ArticleBom GetArticleBom()
        {
            return _dbMasterDataCache.M_ArticleBomGetByArticleChildId(
                new Id(_productionOrderBom.ArticleChildId));
        }

        public void CreateProductionOrderOperation(ProductionOrder parentProductionOrder)
        {
            _productionOrderBom.ProductionOrderOperation = ProductionOrderOperation.CreateProductionOrderOperation(GetArticleBom(),
                parentProductionOrder);
            _productionOrderBom.ProductionOrderOperationId =
                _productionOrderBom.ProductionOrderOperation.Id;
        }
    }
}