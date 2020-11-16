using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using static IJobs;

namespace Master40.SimulationCore.Agents.HubAgent.Types.Queuing
{
    public class JobQueue : Queue<IJob>
    {

        public double Priority(long currentTime)
        {
            return this.Min(x => x.Priority(currentTime));
        }

    }

}
