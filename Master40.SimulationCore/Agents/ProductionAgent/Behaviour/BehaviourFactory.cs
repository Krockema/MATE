using Master40.DB.Enums;
using Master40.SimulationCore.MessageTypes;

namespace Master40.SimulationCore.Agents.ProductionAgent.Behaviour
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
