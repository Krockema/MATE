using Master40.DB.Enums;
using Master40.SimulationCore.Agents.HubAgent.Types;
using Master40.SimulationCore.Types;
using static FOperations;
using static FUpdateStartConditions;

namespace Master40.SimulationCore.Agents.HubAgent.Behaviour
{
    public static class Factory
    {
        public static IBehaviour Get(SimulationType simType)
        {
            switch (simType)
            {
                case SimulationType.DefaultSetup:
                    return DefaultSetup();
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

        private static IBehaviour DefaultSetup()
        {

            return new DefaultSetup();

        }

        private static IBehaviour Bucket()
        {

            return new Bucket();

        }

    }
}
