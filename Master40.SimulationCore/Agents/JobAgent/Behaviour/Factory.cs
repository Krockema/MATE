using Master40.DB.Nominal;
using Master40.SimulationCore.Types;
using static IConfirmations;

namespace Master40.SimulationCore.Agents.JobAgent.Behaviour
{
    public static class Factory
    {
        public static IBehaviour Get(SimulationType simType, IConfirmation jobConfirmation)
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

        private static IBehaviour Default(IConfirmation jobConfirmation)
        { 

            return new Default(jobConfirmation);

        }
    }
}
