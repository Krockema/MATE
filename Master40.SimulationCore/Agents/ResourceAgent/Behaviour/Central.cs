using Master40.DB.Nominal;
using Master40.SimulationCore.Agents.StorageAgent;
using System;
using System.Collections.Generic;
using System.Text;

namespace Master40.SimulationCore.Agents.ResourceAgent.Behaviour
{
    class Central : SimulationCore.Types.Behaviour
    {
        public Central(SimulationType simulationType = SimulationType.None)
            : base(simulationType: simulationType)
        {
        }

        public override bool Action(object message)
        {
            switch (message)
            {
                default: return false;
            }
            return true;
        }

    }
}
