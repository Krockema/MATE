using Master40.DB.Enums;
using Master40.SimulationCore.Types;

namespace Master40.SimulationCore.Agents.HubAgent.Behaviour
{
    public static class Factory
    {
        public static IBehaviour Get(SimulationType simType, long maxBucketSize)
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
                case SimulationType.BucketScope:
                    behaviour = BucketScope(maxBucketSize);
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

        private static IBehaviour BucketScope(long maxBucketSize)
        {

            return new BucketScope(maxBucketSize: maxBucketSize);

        }

    }
}
