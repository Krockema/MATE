using Master40.DB.Enums;
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
                case SimulationType.Bucket: behaviour = Default(simType);
                    break;
                case SimulationType.DefaultSetup: behaviour = Default(simType);
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

    }

}

