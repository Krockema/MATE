using Master40.DB.Nominal;
using Master40.SimulationCore.Agents.ResourceAgent.Types;
using Master40.SimulationCore.Helper.DistributionProvider;
using Master40.SimulationCore.Types;

namespace Master40.SimulationCore.Agents.ResourceAgent.Behaviour
{
    public static class Factory
    {
        public static IBehaviour Get(SimulationType simType, WorkTimeGenerator workTimeGenerator, ToolManager toolManager)
        {
            IBehaviour behaviour;
            switch (simType)
            {
                case SimulationType.DefaultSetup:
                    behaviour = DefaultSetup(workTimeGenerator: workTimeGenerator, toolManager: toolManager);
                    break;
                case SimulationType.DefaultSetupStack:
                    behaviour = DefaultSetupStack(workTimeGenerator: workTimeGenerator, toolManager: toolManager);
                    break;
                case SimulationType.BucketScope:
                    behaviour = BucketScope(workTimeGenerator: workTimeGenerator, toolManager: toolManager);
                    break;
                default:
                    behaviour = Default(workTimeGenerator: workTimeGenerator, toolManager: toolManager);
                    break;
            }

            return behaviour;
        }

        private static IBehaviour Default(WorkTimeGenerator workTimeGenerator, ToolManager toolManager)
        {
            //TODO - create config item.
            return new Default(planingJobQueueLength: 45
                            , fixedJobQueueSize: 1
                            , workTimeGenerator: workTimeGenerator
                            , toolManager: toolManager);

        }

        private static IBehaviour DefaultSetup(WorkTimeGenerator workTimeGenerator, ToolManager toolManager)
        {
            //TODO - create config item.
            return new DefaultSetup(planingJobQueueLength: 45
                , fixedJobQueueSize: 1
                , workTimeGenerator: workTimeGenerator
                , toolManager: toolManager);

        }
        
        private static IBehaviour BucketScope(WorkTimeGenerator workTimeGenerator, ToolManager toolManager)
        {
            //TODO - create config item.
            return new BucketScope(planingJobQueueLength: 45
                , fixedJobQueueSize: 1
                , workTimeGenerator: workTimeGenerator
                , toolManager: toolManager);
        }

        private static IBehaviour DefaultSetupStack(WorkTimeGenerator workTimeGenerator, ToolManager toolManager)
        {
            //TODO - create config item.
            return new DefaultSetupStack(planingJobQueueLength: 45
                , fixedJobQueueSize: 1
                , workTimeGenerator: workTimeGenerator
                , toolManager: toolManager);
        }


    }

}
