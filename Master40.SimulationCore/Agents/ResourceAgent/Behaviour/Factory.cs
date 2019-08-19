using Master40.DB.Enums;
using Master40.SimulationCore.DistributionProvider;
using Master40.SimulationCore.Types;

namespace Master40.SimulationCore.Agents.ResourceAgent.Behaviour
{
    public static class Factory
    {

        public static IBehaviour Get(SimulationType simType, WorkTimeGenerator workTimeGenerator)
        {
            switch (simType)
            {
                default:
                    return Default(workTimeGenerator);
            }
        }

        private static IBehaviour Default(WorkTimeGenerator workTimeGenerator)
        {
            //TODO - create config item.
            return new Default(planingJobQueueLength: 45
                            , fixedJobQueueSize: 1
                            , workTimeGenerator: workTimeGenerator);

        }

    }

}
