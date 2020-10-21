using Master40.DB.Nominal;
using Master40.SimulationCore.Types;

namespace Master40.SimulationCore.Agents.ContractAgent.Behaviour
{
    public static class Factory
    {
        public static IBehaviour Get(SimulationType simType)
        {
            IBehaviour behaviour;
            switch (simType)
            {
                case SimulationType.Default: behaviour = Default();
                    break;
                case SimulationType.Central: behaviour = Central(simType);
                    break;
                default:
                    behaviour = Default();
                    break;
                    
            }
            return behaviour;
        }

        private static IBehaviour Default()
        {
            return new Default();
        }

        private static IBehaviour Central(SimulationType simType)
        {
            return new Central(simType);
        }
    }
}
