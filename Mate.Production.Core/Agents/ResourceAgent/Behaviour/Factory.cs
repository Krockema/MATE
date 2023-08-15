using System;
using System.Collections.Generic;
using Mate.DataCore.DataModel;
using Mate.DataCore.Nominal;
using Mate.DataCore.Nominal.Model;
using Mate.Production.Core.Environment.Records.Central;
using Mate.Production.Core.Helper.DistributionProvider;
using Mate.Production.Core.Types;

namespace Mate.Production.Core.Agents.ResourceAgent.Behaviour
{
    public static class Factory
    {
        public static IBehaviour Get(SimulationType simType, WorkTimeGenerator workTimeGenerator, List<M_ResourceCapabilityProvider> capabilityProvider, TimeSpan timeConstraintQueueLength, int resourceId, ResourceType resourceType)
        {
            IBehaviour behaviour;
            switch (simType)
            {
                case SimulationType.Default:
                    behaviour = Default(workTimeGenerator: workTimeGenerator, capabilityProvider, timeConstraintQueueLength, resourceId, resourceType);
                    break;
                case SimulationType.Queuing:
                    behaviour = Queuing(capabilityProvider, resourceId, resourceType);
                    break;
                default:
                    behaviour = Default(workTimeGenerator: workTimeGenerator, capabilityProvider, timeConstraintQueueLength, resourceId, resourceType);
                    break;
            }

            return behaviour;
        }

        private static IBehaviour Default(WorkTimeGenerator workTimeGenerator, List<M_ResourceCapabilityProvider> capabilityProvider, TimeSpan timeConstraintQueueLength, int resourceId, ResourceType resourceType)
        {
            //TODO - create config item.
            return new Default(timeConstraintQueueLength: timeConstraintQueueLength //480
                            , resourceId: resourceId
                            , resourceType: resourceType
                            , workTimeGenerator: workTimeGenerator
                            , capabilityProvider: capabilityProvider);

        }

        public static IBehaviour Central(CentralResourceDefinitionRecord resourceDefinition)
        {
            return new Central(resourceDefinition, SimulationType.Central);
        }
        private static IBehaviour Queuing(List<M_ResourceCapabilityProvider> capabilityProvider, int resourceId, ResourceType resourceType)
        {
            return new Queuing(resourceId: resourceId, resourceType, capabilityProvider: capabilityProvider);
        }

    }

}
