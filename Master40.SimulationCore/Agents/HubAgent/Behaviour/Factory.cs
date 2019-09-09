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
            IBehaviour behaviour;
            switch (simType)
            {
                case SimulationType.DefaultSetup:
                    behaviour = DefaultSetup();
                    break;
                case SimulationType.Bucket:
                     behaviour = Bucket();
                    break;
                default:
                    behaviour =  Default();
                    break;
            }

            return behaviour;
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
