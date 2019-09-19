using Master40.DB.Enums;
using Master40.SimulationCore.Types;

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
                case SimulationType.DefaultSetupStack:
                    behaviour = DefaultSetup();
                    break;
                case SimulationType.Bucket:
                    behaviour = Bucket();
                    break;
                case SimulationType.BucketScope:
                    behaviour = BucketScope();
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

        private static IBehaviour BucketScope()
        {

            return new BucketScope();

        }

    }
}
