using Mate.DataCore.Nominal;
using Mate.Production.Core.Types;
using static IConfirmations;

namespace Mate.Production.Core.Agents.JobAgent.Behaviour
{
    public static class Factory
    {
        public static IBehaviour Get(SimulationType simType, IConfirmation jobConfirmation)
        {
            IBehaviour behaviour;
            switch (simType)
            {
                case SimulationType.Default:
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
