using Master40.DB.Nominal;
using Master40.SimulationCore.Types;

namespace Master40.SimulationCore.Agents.DispoAgent.Behaviour
{
    public static class Factory
    {
        public static IBehaviour Get(SimulationType simType)
        {
            IBehaviour behaviour;
            switch (simType)
            {
                case SimulationType.Bucket:
                    behaviour = Default();
                    break;
                case SimulationType.DefaultSetup:
                    behaviour = Default();
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

    }
}
