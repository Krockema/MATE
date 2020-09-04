using Master40.DB.Nominal;
using System;

namespace Master40.SimulationCore.Agents.StorageAgent.Behaviour
{
    class Central : SimulationCore.Types.Behaviour
    {

        public Central(SimulationType simType) : base(simulationType: simType)
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
