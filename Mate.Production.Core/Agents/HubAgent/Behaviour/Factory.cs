using Mate.DataCore.Nominal;
using Mate.Production.Core.Helper.DistributionProvider;
using Mate.Production.Core.Types;
using System;

namespace Mate.Production.Core.Agents.HubAgent.Behaviour
{
    public static class Factory
    {
        public static IBehaviour Get(SimulationType simType, TimeSpan maxBucketSize, WorkTimeGenerator workTimeGenerator)
        {
            IBehaviour behaviour;
            switch (simType)
            {
                case SimulationType.Default:
                    behaviour = Default(maxBucketSize, workTimeGenerator);
                    break;
                case SimulationType.Queuing:
                    behaviour = Queuing(maxBucketSize, workTimeGenerator);
                    break;
                default:
                    behaviour = Default(maxBucketSize, workTimeGenerator);
                    break;
            }

            return behaviour;
        }
        
        private static IBehaviour Default(TimeSpan maxBucketSize, WorkTimeGenerator workTimeGenerator)
        {
            return new Default(maxBucketSize: maxBucketSize, workTimeGenerator: workTimeGenerator);
        }


        public static IBehaviour Central(string dbConnectionStringGanttPlan, string dbConnectionStringMaster, string pathToGANTTPLANOptRunner, WorkTimeGenerator workTimeGenerator)
        {
            return new Central(dbConnectionStringGanttPlan, dbConnectionStringMaster, pathToGANTTPLANOptRunner, workTimeGenerator);
        }

        private static IBehaviour Queuing(TimeSpan maxBucketSize, WorkTimeGenerator workTimeGenerator)
        {
            return new Queuing(maxBucketSize: maxBucketSize, workTimeGenerator: workTimeGenerator);
        }
    }
}
