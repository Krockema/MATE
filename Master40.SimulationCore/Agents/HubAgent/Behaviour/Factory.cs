using Master40.DB.Nominal;
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
                    behaviour =  DefaultSetup();
                    break;
            }

            return behaviour;
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
