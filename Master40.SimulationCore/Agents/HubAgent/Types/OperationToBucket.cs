using System;
using static FOperations;

namespace Master40.SimulationCore.Agents.HubAgent.Types
{
    public class OperationToBucket
    {
        public Guid BucketKey { get; set; }
        public FOperation fOperation { get; set; }

        public OperationToBucket(Guid bucketKey, FOperation fOperation)
        {
            BucketKey = bucketKey;
            this.fOperation = fOperation;
        }



    }
}
