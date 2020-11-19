using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using static IJobs;

namespace Master40.SimulationCore.Agents.HubAgent.Types.Queuing
{
    public class JobQueue : List<IJob>
    {
        public IJob PeekNext(long currentTime)
        {
            return this.OrderBy(x => x.Priority(currentTime)).First();
        }

        public IJob DequeueNext(long currentTime)
        {
            var jobToDeuque = this.OrderBy(x => x.Priority(currentTime)).First();
            Remove(jobToDeuque);
            return jobToDeuque;
        }
        public double Priority(long currentTime)
        {
            return this.Min(x => x.Priority(currentTime));
        }

    }

}
