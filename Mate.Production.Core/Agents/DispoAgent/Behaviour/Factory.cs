using Mate.DataCore.Nominal;
using Mate.Production.Core.Types;

namespace Mate.Production.Core.Agents.DispoAgent.Behaviour
{
    public static class Factory
    {
        public static IBehaviour Get(SimulationType simType)
        {
            IBehaviour behaviour;
            switch (simType)
            {
                case SimulationType.Default:
                    behaviour = Default();
                    break;
                case SimulationType.Central:
                    behaviour = Default(simType);
                    break;
                case SimulationType.Queuing:
                    behaviour = Default(simType);
                    break;
                default:
                    behaviour = Default(simType);
                    break;
            }

            return behaviour;
        }

        private static IBehaviour Default(SimulationType simulationType = SimulationType.Default)
        { 

            return new Default(simulationType);

        }

    }
}
