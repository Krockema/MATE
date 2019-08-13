using Master40.DB.Enums;
using Master40.SimulationCore.Types;

namespace Master40.SimulationCore.Agents.ContractAgent.Behaviour
{
    public static class Factory
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
