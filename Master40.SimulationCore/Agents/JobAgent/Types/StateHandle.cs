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
        public bool Setup { get; }
        public bool Processing { get; }

        public StateHandle(JobState currentState, bool setup, bool processing)
        {
            CurrentState = currentState;
            Setup = setup;
            Processing = processing;
        }


    }
}
