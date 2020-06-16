using Master40.DB.DataModel;
using Master40.DB.Nominal;
using Master40.SimulationCore.Agents.ResourceAgent.Types;
using Master40.SimulationCore.Helper.DistributionProvider;
using Master40.SimulationCore.Types;
using System.Collections.Generic;

namespace Master40.SimulationCore.Agents.ResourceAgent.Behaviour
{
    public static class Factory
    {
        public static IBehaviour Get(SimulationType simType, WorkTimeGenerator workTimeGenerator, List<M_ResourceCapabilityProvider> capabilityProvider)
        {
            IBehaviour behaviour;
            switch (simType)
            {
                case SimulationType.Default:
                    behaviour = Default(workTimeGenerator: workTimeGenerator, capabilityProvider);
                    break;
                default:
                    behaviour = Default(workTimeGenerator: workTimeGenerator, capabilityProvider);
                    break;
            }

            return behaviour;
        }

        private static IBehaviour Default(WorkTimeGenerator workTimeGenerator, List<M_ResourceCapabilityProvider> capabilityProvider)
        {
            //TODO - create config item.
            return new Default(planingJobQueueLength: 45
                            , fixedJobQueueSize: 1
                            , workTimeGenerator: workTimeGenerator
                            , capabilityProvider: capabilityProvider);

        }


    }

}
