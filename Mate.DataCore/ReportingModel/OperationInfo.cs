using System;

namespace Mate.DataCore.ReportingModel
{
    public class OperationInfo
    {
       public Guid OperationKey { get; set; }
       public string CapabilityName { get; set; }
       public int OperationsCountAtReady { get; set; }
       public long Start { get; set; } = 0L;
       public long ReadyAt { get; set; } = 0L;
       public long GetIdleTime() => Start - ReadyAt;

        public OperationInfo(Guid key, string capabilityName)
        {
            OperationKey = key;
            CapabilityName = capabilityName;
            OperationsCountAtReady = 0;
        }

        public int SetOperationsCount(int amount)
        {
            OperationsCountAtReady = amount;
            return OperationsCountAtReady;
        }

        public void SetStartAndReadyAt(long start, long readyAt)
        {
            if (Start > 0 && ReadyAt > 0L)
                return;
            
            Start = start;
            ReadyAt = readyAt;
        }

    }
}
