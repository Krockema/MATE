using System;
using System.Collections.Generic;
using System.Text;

namespace Master40.SimulationCore.Agents.ResourceAgent.Types
{
    public class QueueingPosition
    {
        public QueueingPosition(bool isQueueAble = false, long estimatedStart = 0)
        {
            IsQueueAble = isQueueAble;
            EstimatedStart = estimatedStart;
        }
        public long EstimatedStart { get; set; }
        public bool IsQueueAble { get; set; }
    }
}
