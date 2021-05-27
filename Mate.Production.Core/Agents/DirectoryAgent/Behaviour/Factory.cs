using Mate.DataCore.Nominal;
using Mate.Production.Core.Types;

namespace Mate.Production.Core.Agents.DirectoryAgent.Behaviour
{
    public static class Factory
    {
        public static IBehaviour Get(SimulationType simType)
        {
            IBehaviour behaviour;
            switch (simType)
            {
                case SimulationType.Default: behaviour = Default(simType);
                    break;
                case SimulationType.Central: behaviour = Central(simType);
                    break;
                case SimulationType.Queuing: behaviour = Queuing(simType);
                    break;
                default: behaviour = Default(simType);
                    break;
            }
            return behaviour;
        }

        private static IBehaviour Default(SimulationType simType)
        {
            return new Default(simulationType: simType);
        }

        private static IBehaviour Central(SimulationType simType)
        {
            return new Central(simulationType: simType);
        }

        private static IBehaviour Queuing(SimulationType simType)
        {
            return new Queuing(simulationType: simType);
        }

    }

}

