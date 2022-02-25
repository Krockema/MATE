using System;
using System.Collections.Generic;
using System.Linq;

namespace Mate.DataCore.ReportingModel
{
    public class OperationInfoList : List<OperationInfo>
    {
        public int CountBy(string capabilityName)
        {
            return this.Count(x => x.CapabilityName == capabilityName);
        }

        public int SetOperationsCount(Guid operationKey)
        {
            var operationInfo = this.Single(x => x.OperationKey == operationKey);
            var amount = CountBy(operationInfo.CapabilityName);
            return this.Single(x => x.OperationKey == operationKey).SetOperationsCount(amount);
        }
    }
}
