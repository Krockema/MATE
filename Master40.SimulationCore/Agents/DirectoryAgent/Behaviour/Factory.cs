using Master40.DB.Nominal;
using Master40.SimulationCore.Agents.ContractAgent.Behaviour;
using Master40.SimulationCore.Types;

namespace Master40.SimulationCore.Agents.DirectoryAgent.Behaviour
{
    public static class Factory
    {
        public static IBehaviour Get(SimulationType simType)
        {
            IBehaviour behaviour;
            switch (simType)
            {
                case SimulationType.Default: behaviour = Default(simType);
                    break;
                case SimulationType.Central: behaviour = Central(simType);
                    break;
                default: behaviour = Default(simType);
                    break;
            }
            return behaviour;
        }

        private static IBehaviour Default(SimulationType simType)
        {
            return new Default(simulationType: simType);
        }

        private static IBehaviour Central(SimulationType simType)
        {
            return new Central(simulationType: simType);
        }

    }

}

