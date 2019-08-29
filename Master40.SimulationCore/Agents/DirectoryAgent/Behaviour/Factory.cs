using Master40.DB.Enums;
using Master40.SimulationCore.Types;

namespace Master40.SimulationCore.Agents.DirectoryAgent.Behaviour
{
    public static class Factory
    {
        public static IBehaviour Get(SimulationType simType)
        {
            IBehaviour behave;
            switch (simType)
            {
                case SimulationType.DefaultSetup: behave = Default(simType); break;    
                default: behave = Default(simType); break;
            }
            return behave;
        }

        private static IBehaviour Default(SimulationType simType)
        {
            return new Default(simulationType: simType);
        }

    }

}

