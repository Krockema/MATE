using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static FBuckets;

namespace Mate.Production.Core.Agents.HubAgent.Types
{
    internal class StabilityManager : Dictionary<Guid, List<OperationPosition>>
    {
        void AddEntryForBucket(FBucket bucket, int position, string resource)
        {
            foreach(var operation in bucket.Operations)
            {
                AddEntryForOperation(operation.Key, position, resource);
            }
        }

        void AddEntryForOperation(Guid operationKey, int position, string resource)
        {
            OperationPosition operationPosition = new(position, resource);

            if (this.TryGetValue(operationKey, out var operationPositionList))
            {
                operationPositionList.Add(operationPosition);
            }
            else
            {
                this.Add(operationKey, new List<OperationPosition>((IEnumerable<OperationPosition>)operationPosition));
            }
        }


        //Anything to export to csv whatever
         
        //Create mean over values


    }
}
