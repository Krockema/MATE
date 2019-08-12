using Master40.DB.Enums;
using Master40.SimulationCore.MessageTypes;
using System;
using System.Collections.Generic;
using System.Text;

namespace Master40.SimulationCore.Agents.HubAgent.Behaviour
{
    public static class BehaviourFactory
    {
        public static IBehaviour Get(SimulationType simType)
        {
            switch (simType)
            {
                default:
                    return Default();
            }
        }

        private static IBehaviour Default()
        {

            return new Default();

        }

    }
}
