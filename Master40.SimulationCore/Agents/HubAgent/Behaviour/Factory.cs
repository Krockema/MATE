using Master40.DB.Nominal;
using Master40.SimulationCore.Helper.DistributionProvider;
using Master40.SimulationCore.Types;

namespace Master40.SimulationCore.Agents.HubAgent.Behaviour
{
    public static class Factory
    {
        public static IBehaviour Get(SimulationType simType, long maxBucketSize, WorkTimeGenerator workTimeGenerator)
        {
            IBehaviour behaviour;
            switch (simType)
            {
                case SimulationType.Default:
                    behaviour = Default(maxBucketSize, workTimeGenerator);
                    break;
                default:
                    behaviour = Default(maxBucketSize, workTimeGenerator);
                    break;
            }

            return behaviour;
        }
        
        private static IBehaviour Default(long maxBucketSize, WorkTimeGenerator workTimeGenerator)
        {
            return new Default(maxBucketSize: maxBucketSize, workTimeGenerator: workTimeGenerator);
        }

    }
}
