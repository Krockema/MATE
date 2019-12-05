using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.EntityFrameworkCore.Migrations.Operations;
using static FOperations;

namespace Master40.SimulationCore.Agents.HubAgent.Types
{
    public class PendingOperationDictionary
    {
        private HashSet<OperationToBucket> _pendingOperationDictionary { get; } = new HashSet<OperationToBucket>();

        public void AddOperation(Guid bucketKey, FOperation fOperation)
        {
            var operationToBucket = new OperationToBucket(bucketKey, fOperation);
            _pendingOperationDictionary.Add(operationToBucket);
        }

        public List<OperationToBucket> GetAllOperationsForBucket(Guid bucketKey)
        {
            var operationsForBucket = _pendingOperationDictionary.Where(x => x.BucketKey == bucketKey).ToList();
            return operationsForBucket;
        }

        public void Remove(OperationToBucket operationToBucket)
        {
            _pendingOperationDictionary.Remove(operationToBucket);
        }
    }
}
