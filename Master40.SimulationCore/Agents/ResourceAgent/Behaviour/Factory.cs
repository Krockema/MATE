using Master40.DB.DataModel;
using Master40.DB.Nominal;
using Master40.SimulationCore.Helper.DistributionProvider;
using Master40.SimulationCore.Types;
using System.Collections.Generic;

namespace Master40.SimulationCore.Agents.ResourceAgent.Behaviour
{
    public static class Factory
    {
        public static IBehaviour Get(SimulationType simType, WorkTimeGenerator workTimeGenerator, List<M_ResourceCapabilityProvider> capabilityProvider, int timeConstraintQueueLength, int resourceId)
        {
            IBehaviour behaviour;
            switch (simType)
            {
                case SimulationType.Default:
                    behaviour = Default(workTimeGenerator: workTimeGenerator, capabilityProvider, timeConstraintQueueLength, resourceId);
                    break;
                case SimulationType.Central:
                    behaviour = Central();
                    break;
                default:
                    behaviour = Default(workTimeGenerator: workTimeGenerator, capabilityProvider, timeConstraintQueueLength, resourceId);
                    break;
            }

            return behaviour;
        }

        private static IBehaviour Default(WorkTimeGenerator workTimeGenerator, List<M_ResourceCapabilityProvider> capabilityProvider, int timeConstraintQueueLength, int resourceId)
        {
            //TODO - create config item.
            return new Default(timeConstraintQueueLength: timeConstraintQueueLength //480
                            , resourceId: resourceId
                            , workTimeGenerator: workTimeGenerator
                            , capabilityProvider: capabilityProvider);

        }

        private static IBehaviour Central()
        {
            return new Central();
        }

    }

}
