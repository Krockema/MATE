using System;
using System.Collections.Generic;
using Master40.DB.Data.WrappersForPrimitives;
using Master40.DB.DataModel;
using Master40.DB.Nominal;
using Zpp.DataLayer.impl.WrapperForEntities;
using Zpp.Mrp2.impl.Scheduling.impl;
using Zpp.Mrp2.impl.Scheduling.impl.JobShopScheduler;
using Zpp.Scheduling.impl;
using Zpp.Util;
using Zpp.Util.Graph;
using Zpp.Util.Graph.impl;

namespace Zpp.DataLayer.impl.ProviderDomain.Wrappers
{
    public class ProductionOrderOperation : IScheduleNode
    {
        private readonly T_ProductionOrderOperation _productionOrderOperation;
        private Priority _priority = null;

        public ProductionOrderOperation(T_ProductionOrderOperation productionOrderOperation)
        {
            _productionOrderOperation = productionOrderOperation;
        }

        public T_ProductionOrderOperation GetValue()
        {
            return _productionOrderOperation;
        }

        public Id GetResourceCapabilityId()
        {
            return new Id(_productionOrderOperation.ResourceCapabilityId);
        }

        public void SetMachine(WrapperForEntities.Resource resource)
        {
            if (_productionOrderOperation.IsReadOnly)
            {
                throw new MrpRunException("A readOnly entity cannot be changed anymore.");
            }

            _productionOrderOperation.ResourceId = resource.GetValue().Id;
            _productionOrderOperation.Resource = resource.GetValue();
        }

        private void EnsureMachineIsLoaded()
        {
            if (_productionOrderOperation.Resource == null)
            {
                if (_productionOrderOperation.ResourceId == null)
                {
                    throw new MrpRunException(
                        "Cannot load Machine, if this operation is not yet planned.");
                }

                Id resourceId = new Id(_productionOrderOperation.ResourceId.GetValueOrDefault());
                IDbMasterDataCache dbMasterDataCache =
                    ZppConfiguration.CacheManager.GetMasterDataCache();
                WrapperForEntities.Resource resource = dbMasterDataCache.M_ResourceGetById(resourceId);
                _productionOrderOperation.Resource = resource.GetValue();
            }
        }

        public Id GetMachineId()
        {
            if (_productionOrderOperation.ResourceId == null)
            {
                throw new MrpRunException(
                    "Cannot get Machine, if this operation is not yet planned.");
            }

            return new Id(_productionOrderOperation.ResourceId.GetValueOrDefault());
        }

        public M_Resource GetMachine()
        {
            EnsureMachineIsLoaded();
            return _productionOrderOperation.Resource;
        }

        public List<WrapperForEntities.Resource> GetPossibleMachines()
        {
            return ZppConfiguration.CacheManager.GetAggregator()
                .GetResourcesByResourceCapabilityId(this.GetResourceCapabilityId());
        }

        public override bool Equals(object obj)
        {
            if (obj.GetType() != typeof(ProductionOrderOperation))
            {
                return false;
            }

            ProductionOrderOperation productionOrderOperation = (ProductionOrderOperation) obj;
            return _productionOrderOperation.GetId()
                .Equals(productionOrderOperation._productionOrderOperation.GetId());
        }

        public override int GetHashCode()
        {
            return _productionOrderOperation.Id.GetHashCode();
        }

        public HierarchyNumber GetHierarchyNumber()
        {
            return _productionOrderOperation.GetHierarchyNumber();
        }

        public Duration GetDuration()
        {
            return new Duration(_productionOrderOperation.Duration);
        }

        public Id GetId()
        {
            return _productionOrderOperation.GetId();
        }

        public NodeType GetNodeType()
        {
            return NodeType.Operation;
        }

        public Id GetProductionOrderId()
        {
            return new Id(_productionOrderOperation.ProductionOrderId);
        }

        public override string ToString()
        {
            string state = Enum.GetName(typeof(State), GetState());
            return
                $"{_productionOrderOperation.GetId()}: {_productionOrderOperation.Name}; {state}; IsReadOnly: {IsReadOnly()}; "
                + $"bs/be: {_productionOrderOperation.StartBackward}/{_productionOrderOperation.EndBackward}; " +
                $"s/e: {_productionOrderOperation.Start}/{_productionOrderOperation.End}; ResourceId: {_productionOrderOperation.ResourceId}";
        }

        public void SetPriority(Priority priority)
        {
            if (_productionOrderOperation.IsReadOnly)
            {
                throw new MrpRunException("A readOnly entity cannot be changed anymore.");
            }

            _priority = priority;
        }

        public Priority GetPriority()
        {
            return _priority;
        }

        public ProductionOrder GetProductionOrder()
        {
            IDbTransactionData dbTransactionData =
                ZppConfiguration.CacheManager.GetDbTransactionData();
            return dbTransactionData.ProductionOrderGetById(GetProductionOrderId());
        }

        /**
         * Every operation needs material to start.
         * @returns the time when material of this operation is available
         */
        public DueTime GetEarliestPossibleStartTime()
        {
            IAggregator aggregator = ZppConfiguration.CacheManager.GetAggregator();
            
                DueTime earliestDueTime =
                    aggregator.GetEarliestPossibleStartTimeOf(
                        this);

            return earliestDueTime;
        }

        public void SetFinished()
        {
            _productionOrderOperation.State = State.Finished;
        }

        public void SetInProgress()
        {
            if (_productionOrderOperation.State.Equals(State.Finished))
            {
                throw new MrpRunException("Impossible, the operation is already finished.");
            }

            _productionOrderOperation.State = State.InProgress;
        }

        public DueTime GetEndTimeBackward()
        {
            if (_productionOrderOperation.EndBackward == null)
            {
                throw new MrpRunException("Cannot request endTime before operation is scheduled.");
            }

            return new DueTime(_productionOrderOperation.EndBackward.GetValueOrDefault());
        }


        public DueTime GetStartTimeBackward()
        {
            if (_productionOrderOperation.StartBackward == null)
            {
                return null;
            }

            DueTime transitionTime =
                new DueTime(TransitionTimer.CalculateTransitionTime(GetDuration()));
            DueTime startTimeOfOperation =
                new DueTime(_productionOrderOperation.StartBackward.GetValueOrDefault());
            DueTime startTime = startTimeOfOperation.Minus(transitionTime);
            return startTime;
        }

        public void SetStartTimeBackward(DueTime startTime)
        {
            if (_productionOrderOperation.IsReadOnly)
            {
                throw new MrpRunException("A readOnly entity cannot be changed anymore.");
            }

            DueTime transitionTime =
                new DueTime(TransitionTimer.CalculateTransitionTime(GetDuration()));
            // startBackwards
            DueTime startTimeOfOperation = startTime.Plus(transitionTime);
            _productionOrderOperation.StartBackward = startTimeOfOperation.GetValue();
            // endBackwards
            _productionOrderOperation.EndBackward =
                startTimeOfOperation.GetValue() + GetDuration().GetValue();
        }

        Duration IScheduleNode.GetDuration()
        {
            return _productionOrderOperation.GetDuration();
        }

        public bool IsFinished()
        {
            return _productionOrderOperation.State.Equals(State.Finished);
        }

        public bool IsInProgress()
        {
            return _productionOrderOperation.State.Equals(State.InProgress);
        }

        public int GetStartTime()
        {
            return _productionOrderOperation.Start;
        }

        public int GetEndTime()
        {
            return _productionOrderOperation.End;
        }

        public void SetStartTime(int startTime)
        {
            if (_productionOrderOperation.IsReadOnly)
            {
                throw new MrpRunException("A readOnly entity cannot be changed anymore.");
            }

            _productionOrderOperation.Start = startTime;
        }

        public void SetEndTime(int endTime)
        {
            if (_productionOrderOperation.IsReadOnly)
            {
                throw new MrpRunException("A readOnly entity cannot be changed anymore.");
            }

            _productionOrderOperation.End = endTime;
        }

        public void SetEndTimeBackward(DueTime endTime)
        {
            if (_productionOrderOperation.IsReadOnly)
            {
                throw new MrpRunException("A readOnly entity cannot be changed anymore.");
            }

            // endBackwards
            _productionOrderOperation.EndBackward = endTime.GetValue();
            // startBackwards
            DueTime startTimeOfOperation = endTime.Minus(GetDuration().ToDueTime());
            _productionOrderOperation.StartBackward = startTimeOfOperation.GetValue();
        }

        public void ClearStartTimeBackward()
        {
            if (_productionOrderOperation.IsReadOnly)
            {
                throw new MrpRunException("A readOnly entity cannot be changed anymore.");
            }

            _productionOrderOperation.StartBackward = null;
        }

        public void ClearEndTimeBackward()
        {
            if (_productionOrderOperation.IsReadOnly)
            {
                throw new MrpRunException("A readOnly entity cannot be changed anymore.");
            }

            _productionOrderOperation.EndBackward = null;
        }

        /**
         * This is only for initial scheduling within mrp1
         */
        public OperationBackwardsSchedule ScheduleBackwards(
            OperationBackwardsSchedule lastOperationBackwardsSchedule,
            DueTime dueTimeOfProductionOrder)
        {
            OperationBackwardsSchedule newOperationBackwardsSchedule;

            // case: first run
            if (lastOperationBackwardsSchedule == null)
            {
                newOperationBackwardsSchedule = new OperationBackwardsSchedule(
                    dueTimeOfProductionOrder, _productionOrderOperation.GetDuration(),
                    _productionOrderOperation.GetHierarchyNumber());
            }
            // case: equal hierarchyNumber --> PrOO runs in parallel
            else if (_productionOrderOperation.GetHierarchyNumber()
                .Equals(lastOperationBackwardsSchedule.GetHierarchyNumber()))
            {
                newOperationBackwardsSchedule = new OperationBackwardsSchedule(
                    lastOperationBackwardsSchedule.GetEndOfOperation(),
                    _productionOrderOperation.GetDuration(),
                    _productionOrderOperation.GetHierarchyNumber());
            }
            // case: greaterHierarchyNumber --> PrOO runs after the last PrOO
            else
            {
                if (lastOperationBackwardsSchedule.GetHierarchyNumber()
                    .IsSmallerThan(_productionOrderOperation.GetHierarchyNumber()))
                {
                    throw new MrpRunException(
                        "This is not allowed: hierarchyNumber of lastBackwardsSchedule " +
                        "is smaller than hierarchyNumber of current PrOO (wasn't sorted ?').");
                }

                newOperationBackwardsSchedule = new OperationBackwardsSchedule(
                    lastOperationBackwardsSchedule.GetStartOfOperation(),
                    _productionOrderOperation.GetDuration(),
                    _productionOrderOperation.GetHierarchyNumber());
            }

            // apply schedule on operation
            _productionOrderOperation.EndBackward =
                newOperationBackwardsSchedule.GetEndBackwards().GetValue();
            _productionOrderOperation.StartBackward =
                newOperationBackwardsSchedule.GetStartBackwards().GetValue();

            return newOperationBackwardsSchedule;
        }

        public State? GetState()
        {
            return _productionOrderOperation.State;
        }

        public void SetReadOnly()
        {
            _productionOrderOperation.IsReadOnly = true;
        }

        public bool IsReadOnly()
        {
            return _productionOrderOperation.IsReadOnly;
        }
    }
}