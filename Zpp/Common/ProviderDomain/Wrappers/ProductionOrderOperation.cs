using System.Collections.Generic;
using Master40.DB.Data.WrappersForPrimitives;
using Master40.DB.DataModel;
using Zpp.DbCache;
using Zpp.Mrp.MachineManagement;
using Zpp.Mrp.Scheduling;
using Zpp.OrderGraph;
using Zpp.Utils;
using Zpp.WrappersForPrimitives;

namespace Zpp.Common.ProviderDomain.Wrappers
{
    public class ProductionOrderOperation : INode
    {
        // TODO: Anywhere else needed? -> should be configurable
        private const int TRANSITION_TIME_FACTOR = 3;
        private readonly T_ProductionOrderOperation _productionOrderOperation;
        private readonly IDbMasterDataCache _dbMasterDataCache;
        private Priority _priority = null;

        public ProductionOrderOperation(T_ProductionOrderOperation productionOrderOperation,
            IDbMasterDataCache dbMasterDataCache)
        {
            _productionOrderOperation = productionOrderOperation;
            _dbMasterDataCache = dbMasterDataCache;
        }

        public OperationBackwardsSchedule ScheduleBackwards(
            OperationBackwardsSchedule lastOperationBackwardsSchedule, DueTime dueTimeOfProductionOrder)
        {
            DueTime TIME_BETWEEN_OPERATIONS =
                new DueTime(_productionOrderOperation.Duration * TRANSITION_TIME_FACTOR);
            int? startBackwards;
            int? endBackwards;
            
            // case: equal hierarchyNumber --> PrOO runs in parallel
            if (lastOperationBackwardsSchedule.GetHierarchyNumber() == null ||
                (lastOperationBackwardsSchedule.GetHierarchyNumber() != null &&
                 _productionOrderOperation.HierarchyNumber.Equals(
                     lastOperationBackwardsSchedule.GetHierarchyNumber().GetValue())))
            {
                endBackwards = lastOperationBackwardsSchedule.GetEndBackwards().GetValue();
                startBackwards = endBackwards -
                                 _productionOrderOperation.Duration;
            }
            // case: greaterHierarchyNumber --> PrOO runs after the last PrOO
            else
            {
                if (lastOperationBackwardsSchedule.GetHierarchyNumber().GetValue() <
                    _productionOrderOperation.HierarchyNumber)
                {
                    throw new MrpRunException(
                        "This is not allowed: hierarchyNumber of lastBackwardsSchedule " +
                        "is smaller than hierarchyNumber of current PrOO.");
                }

                endBackwards = lastOperationBackwardsSchedule.GetStartBackwards().GetValue();
                startBackwards = endBackwards -
                                 _productionOrderOperation.Duration;
            }

            // create return value
            
            // skip additional slack time if dueTime is already exceeded
            if (endBackwards > dueTimeOfProductionOrder.GetValue())
            {
                TIME_BETWEEN_OPERATIONS = DueTime.Null();
            }

            OperationBackwardsSchedule newOperationBackwardsSchedule =
                new OperationBackwardsSchedule(new DueTime(endBackwards.GetValueOrDefault()),
                    // slack time aka timeBetweenOperations
                    new DueTime(startBackwards.GetValueOrDefault() -
                                TIME_BETWEEN_OPERATIONS.GetValue()),
                    new HierarchyNumber(
                        _productionOrderOperation.HierarchyNumber));

            _productionOrderOperation.EndBackward = endBackwards;
            _productionOrderOperation.StartBackward = startBackwards;
            _productionOrderOperation.Start = startBackwards.GetValueOrDefault();
            _productionOrderOperation.End = endBackwards.GetValueOrDefault();
            
            return newOperationBackwardsSchedule;
        }

        public T_ProductionOrderOperation GetValue()
        {
            return _productionOrderOperation;
        }

        public Id GetMachineGroupId()
        {
            return new Id(_productionOrderOperation.MachineGroupId);
        }

        public void SetMachine(Machine machine)
        {
            _productionOrderOperation.Machine = machine.GetValue();
        }

        /// <summary>
        /// Todo: Rename to GetPossibleMachines prevent irritation
        /// </summary>
        /// <param name="dbTransactionData"></param>
        /// <returns></returns>
        public List<Machine> GetMachines(IDbTransactionData dbTransactionData)
        {
            return dbTransactionData.GetAggregator().GetMachinesOfProductionOrderOperation(this);
        }

        public override bool Equals(object obj)
        {
            if (obj.GetType()!=typeof(ProductionOrderOperation))
            {
                return false;
            }
            ProductionOrderOperation productionOrderOperation = (ProductionOrderOperation) obj;
            return _productionOrderOperation.GetId().Equals(productionOrderOperation._productionOrderOperation.GetId());
        }

        public override int GetHashCode()
        {
            return _productionOrderOperation.Id.GetHashCode();
        }

        public HierarchyNumber GetHierarchyNumber()
        {
            return new HierarchyNumber(_productionOrderOperation.HierarchyNumber);
        }

        public DueTime GetDuration()
        {
            return new DueTime(_productionOrderOperation.Duration);
        }

        public Id GetId()
        {
            return _productionOrderOperation.GetId();
        }

        public NodeType GetNodeType()
        {
            return NodeType.Operation;
        }

        public INode GetEntity()
        {
            return this;
        }

        public string GetGraphizString(IDbTransactionData dbTransactionData)
        {
            return $"{_productionOrderOperation.Name}";
        }

        public Id GetProductionOrderId()
        {
            if (_productionOrderOperation.ProductionOrderId == null)
            {
                return null;
            }
            return new Id(_productionOrderOperation.ProductionOrderId.GetValueOrDefault());
        }

        public override string ToString()
        {
            return $"{_productionOrderOperation.GetId()}: {_productionOrderOperation.Name}";
        }

        public void SetPriority(Priority priority)
        {
            _priority = priority;
        }

        public Priority GetPriority()
        {
            return _priority;
        }

        public DueTime GetDueTime(IDbTransactionData dbTransactionData)
        {
            DueTime dueTime;
            if (_productionOrderOperation.EndBackward != null)
            {
                dueTime = new DueTime(_productionOrderOperation.EndBackward.GetValueOrDefault());
            }
            else
            {
                // every productionOrderBom whith this operation o1 has the same dueTime
                dueTime = dbTransactionData.GetAggregator()
                    .GetAnyProductionOrderBomByProductionOrderOperation(this)
                    .GetDueTime(dbTransactionData);
            }
            return dueTime;
        }

        public ProductionOrder GetProductionOrder(IDbTransactionData dbTransactionData)
        {
            return dbTransactionData.ProductionOrderGetById(GetProductionOrderId());
        }
    }
}