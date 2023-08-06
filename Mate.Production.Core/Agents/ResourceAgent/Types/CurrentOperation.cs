using Akka.Hive.Definitions;
using System;

namespace Mate.Production.Core.Agents.ResourceAgent.Types
{
    public class CurrentOperation
    {

        public OperationRecord Operation { get; set; } = null;

        public DateTime StartAt { get; set; } = Time.ZERO.Value;

        public void Set(OperationRecord operation, DateTime currentTime)
        {
            Operation = operation;
            StartAt = currentTime;
        }

        public void Reset()
        {
            Operation = null;
            StartAt = Time.ZERO.Value;
        }
    }
}
