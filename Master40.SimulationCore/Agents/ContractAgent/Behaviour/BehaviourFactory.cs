using Akka.Actor;
using Master40.DB.Enums;
using Master40.SimulationCore.MessageTypes;
using System.Collections.Generic;


namespace Master40.SimulationCore.Agents.ContractAgent.Behaviour
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
