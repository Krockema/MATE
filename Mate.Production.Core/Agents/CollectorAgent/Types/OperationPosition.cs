using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mate.Production.Core.Agents.CollectorAgent.Types
{
    public enum Process
    {
        Dequeue,
        Enqueue
    }

    public class OperationPosition
    {
        public long Time { get; private set; }
        public int Position { get; private set; }
        public string Resource { get; private set; }
        public long Start { get; private set; }
        public string Process { get; private set; }
        public long RealTime { get; }

        public OperationPosition(long time, int position, string resource, long start, string process)
        {
            Time = time;
            Position = position;
            Resource = resource;
            Start = start;
            Process = process;
            RealTime = DateTime.Now.Ticks;
        }

    }
}
