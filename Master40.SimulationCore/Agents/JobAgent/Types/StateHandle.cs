using Master40.DB.Nominal;
using NLog.LayoutRenderers;
using System;
using System.Collections.Generic;
using System.Text;

namespace Master40.SimulationCore.Agents.JobAgent.Types
{
    public class StateHandle
    {
        public JobState CurrentState { get; set; }

        public StateHandle(JobState currentState)
        {
            CurrentState = currentState;
        }

    }
}
