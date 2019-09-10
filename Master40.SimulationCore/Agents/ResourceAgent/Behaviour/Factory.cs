using Master40.DB.Enums;
using Master40.SimulationCore.Agents.ResourceAgent.Types;
using Master40.SimulationCore.DistributionProvider;
using Master40.SimulationCore.Types;

namespace Master40.SimulationCore.Agents.ResourceAgent.Behaviour
{
    public static class Factory
    {
        public static IBehaviour Get(SimulationType simType, WorkTimeGenerator workTimeGenerator, int resourceId, ToolManager toolManager)
        {
            switch (simType)
            {
                case SimulationType.DefaultSetup:
                    return DefaultSetup(workTimeGenerator: workTimeGenerator, resourceId: resourceId, toolManager: toolManager);
                case SimulationType.Bucket:
                    return Bucket(workTimeGenerator: workTimeGenerator, resourceId: resourceId, toolManager: toolManager);
                default:
                    return Default(workTimeGenerator: workTimeGenerator, resourceId: resourceId, toolManager: toolManager);
            }
        }

        private static IBehaviour Default(WorkTimeGenerator workTimeGenerator, int resourceId, ToolManager toolManager)
        {
            //TODO - create config item.
            return new Default(planingJobQueueLength: 45
                            , fixedJobQueueSize: 1
                            , workTimeGenerator: workTimeGenerator
                            , toolManager: toolManager);

        }

        private static IBehaviour DefaultSetup(WorkTimeGenerator workTimeGenerator, int resourceId, ToolManager toolManager)
        {
            //TODO - create config item.
            return new DefaultSetup(planingJobQueueLength: 45
                , fixedJobQueueSize: 1
                , workTimeGenerator: workTimeGenerator
                , resourceId: resourceId
                , toolManager: toolManager);

        }

        private static IBehaviour Bucket(WorkTimeGenerator workTimeGenerator, int resourceId, ToolManager toolManager)
        {
            //TODO - create config item.
            return new Bucket(planingJobQueueLength: 45
                , fixedJobQueueSize: 1
                , workTimeGenerator: workTimeGenerator
                , resourceId: resourceId
                , toolManager: toolManager);
        }

    }

}
