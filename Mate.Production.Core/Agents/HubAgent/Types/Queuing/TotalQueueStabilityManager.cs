using System.Collections.Generic;
using System.Linq;
using static IJobs;

namespace Mate.Production.Core.Agents.HubAgent.Types.Queuing
{
    internal class TotalQueueStabilityManager
    {
        List<IJob> _jobs = new();

        public int AddJob(IJob job, long time) { 
            _jobs.Add(job);
            return GetIndexOf(job, time);
        }

        private int GetIndexOf(IJob job, long time) {
            return _jobs.OrderBy(x => x.Priority(time)).ToList().IndexOf(job);
        }

        public int RemoveJob(IJob job, long time) {
            var pos = GetIndexOf(job, time);
            _jobs.RemoveAt(pos);
            return pos;
        }
    }
}
