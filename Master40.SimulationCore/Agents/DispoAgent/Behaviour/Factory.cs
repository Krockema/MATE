using Master40.DB.Enums;
using Master40.SimulationCore.Types;

namespace Master40.SimulationCore.Agents.DispoAgent.Behaviour
{
    public static class Factory
    {
        public static IBehaviour Get(SimulationType simType)
        {
            switch (simType)
            {
                case SimulationType.Bucket:
                    return Bucket();
                default:
                    return Default();
            }
        }

        private static IBehaviour Default()
        { 

            return new Default();

        }

        private static IBehaviour Bucket()
        {

            return new Bucket();

        }
    }
}
