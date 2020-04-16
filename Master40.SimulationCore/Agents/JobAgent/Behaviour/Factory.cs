using Master40.DB.Nominal;
using Master40.SimulationCore.Agents.HubAgent.Types;
using Master40.SimulationCore.Types;

namespace Master40.SimulationCore.Agents.JobAgent.Behaviour
{
    public static class Factory
    {
        public static IBehaviour Get(SimulationType simType, JobConfirmation jobConfirmation)
        {
            IBehaviour behaviour;
            switch (simType)
            {
                case SimulationType.DefaultSetup:
                    behaviour = Default(jobConfirmation);
                    break;
                default:
                    behaviour = Default(jobConfirmation);
                    break;
            }

            return behaviour;
        }

        private static IBehaviour Default(JobConfirmation jobConfirmation)
        { 

            return new Default(jobConfirmation);

        }
    }
}
