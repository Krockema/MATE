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
        public ProductionOrderBom(IDemand demand, IDbMasterDataCache dbMasterDataCache) : base(
            demand, dbMasterDataCache)
        {
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="articleBom"></param>
        /// <param name="parentProductionOrder"></param>
        /// <param name="dbMasterDataCache"></param>
        /// <param name="quantity">of production article to produce
        /// --> is used for childs as: articleBom.Quantity * quantity</param>
        /// <returns></returns>
        public static ProductionOrderBom CreateProductionOrderBom(M_ArticleBom articleBom,
            Provider parentProductionOrder, IDbMasterDataCache dbMasterDataCache, Quantity quantity)
        {
            T_ProductionOrderBom productionOrderBom = new T_ProductionOrderBom();
            // TODO: Terminierung+Maschinenbelegung
            productionOrderBom.Quantity = articleBom.Quantity * quantity.GetValue();
            productionOrderBom.State = State.Created;
            productionOrderBom.ProductionOrderParent =
                (T_ProductionOrder) parentProductionOrder.ToIProvider();
            productionOrderBom.ProductionOrderParentId =
                productionOrderBom.ProductionOrderParent.Id;
            // bom is toPurchase if it's null
            if (articleBom.Operation != null)
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
            return (T_ProductionOrderBom) _demand;
        }

        public override M_Article GetArticle()
        {
            Id articleId = new Id(((T_ProductionOrderBom) _demand).ArticleChildId);
            return _dbMasterDataCache.M_ArticleGetById(articleId);
        }

        /**
         * @return:
         *   if ProductionOrderOperation is backwardsScheduled --> startBackward
         *   else ProductionOrderParent.dueTime
         */
        public override DueTime GetDueTime(IDbTransactionData dbTransactionData)
        {
            T_ProductionOrderBom productionOrderBom = ((T_ProductionOrderBom) _demand);

            // load ProductionOrderOperation if not done yet
            if (productionOrderBom.ProductionOrderOperation == null)
            {
                Id productionOrderOperationId =
                    new Id(productionOrderBom.ProductionOrderOperationId.GetValueOrDefault());
                productionOrderBom.ProductionOrderOperation = dbTransactionData
                    .ProductionOrderOperationGetById(productionOrderOperationId);
            }


            DueTime dueTime;
            if (productionOrderBom.ProductionOrderOperation != null &&
                productionOrderBom.ProductionOrderOperation.StartBackward != null)
            {
                // backwards scheduling was already done --> return StartBackward
                dueTime = new DueTime(productionOrderBom.ProductionOrderOperation.StartBackward
                    .GetValueOrDefault());
                return dueTime;
            }
            // backwards scheduling was not yet done --> return dueTime of ProductionOrderParent

            if (productionOrderBom.ProductionOrderParent == null)
            {
                Id productionOrderId = new Id(productionOrderBom.ProductionOrderParentId);
                productionOrderBom.ProductionOrderParent = (T_ProductionOrder) dbTransactionData
                    .ProvidersGetById(productionOrderId).ToIProvider();
            }

            dueTime = new DueTime(productionOrderBom.ProductionOrderParent.DueTime);
            return dueTime;
        }

        public override string GetGraphizString(IDbTransactionData dbTransactionData)
        {
            // Demand(CustomerOrder);20;Truck

            string graphizString;
            T_ProductionOrderBom tProductionOrderBom = ((T_ProductionOrderBom) _demand);
            if (tProductionOrderBom.ProductionOrderOperationId != null)
            {
                if (tProductionOrderBom.ProductionOrderOperation == null)
                {
                    tProductionOrderBom.ProductionOrderOperation =
                        dbTransactionData.ProductionOrderOperationGetById(new Id(tProductionOrderBom
                            .ProductionOrderOperationId.GetValueOrDefault()));
                }

                T_ProductionOrderOperation tProductionOrderOperation =
                    tProductionOrderBom.ProductionOrderOperation;
                graphizString = $"D(PrOB);{base.GetGraphizString(dbTransactionData)};" +
                                $"bs({tProductionOrderOperation.StartBackward});" +
                                $"be({tProductionOrderOperation.EndBackward})";
            }
            else
            {
                graphizString = $"D(PrOB);{base.GetGraphizString(dbTransactionData)}";
            }

            return graphizString;
        }

        public bool HasOperation()
        {
            return ((T_ProductionOrderBom) _demand).ProductionOrderOperation != null;
        }

        public OperationBackwardsSchedule ScheduleBackwards(
            OperationBackwardsSchedule lastOperationBackwardsSchedule)
        {
            T_ProductionOrderBom tProductionOrderBom = (T_ProductionOrderBom) _demand;
            int? startBackwards;
            int? endBackwards;
            // case: equal hierachyNumber --> PrOO runs in parallel
            if (lastOperationBackwardsSchedule.HierarchyNumber == null ||
                (lastOperationBackwardsSchedule.HierarchyNumber != null &&
                 tProductionOrderBom.ProductionOrderOperation.HierarchyNumber.Equals(
                     lastOperationBackwardsSchedule.HierarchyNumber.GetValue())))
            {
                endBackwards = lastOperationBackwardsSchedule.EndBackwards.GetValue();
                startBackwards = lastOperationBackwardsSchedule.EndBackwards.GetValue() -
                                 tProductionOrderBom.ProductionOrderOperation.Duration;
            }
            // case: greaterHierachyNumer --> PrOO runs after the last PrOO
            else
            {
                if (lastOperationBackwardsSchedule.HierarchyNumber.GetValue() >
                    tProductionOrderBom.ProductionOrderOperation.HierarchyNumber)
                {
                    throw new MrpRunException(
                        "This is not allowed: hierarchyNumber of lastBackwardsSchedule " +
                        "is greater than hierarchyNumber of current PrOO.");
                }

                endBackwards = lastOperationBackwardsSchedule.StartBackwards.GetValue();
                startBackwards = lastOperationBackwardsSchedule.StartBackwards.GetValue() -
                                 tProductionOrderBom.ProductionOrderOperation.Duration;
            }

            tProductionOrderBom.ProductionOrderOperation.EndBackward = endBackwards;
            tProductionOrderBom.ProductionOrderOperation.StartBackward = startBackwards;

            // create return value
            OperationBackwardsSchedule newOperationBackwardsSchedule =
                new OperationBackwardsSchedule();
            newOperationBackwardsSchedule.StartBackwards =
                new DueTime(startBackwards.GetValueOrDefault());
            newOperationBackwardsSchedule.EndBackwards =
                new DueTime(endBackwards.GetValueOrDefault());
            newOperationBackwardsSchedule.HierarchyNumber = new HierarchyNumber(tProductionOrderBom
                .ProductionOrderOperation.HierarchyNumber);

            return newOperationBackwardsSchedule;
        }
    }
}