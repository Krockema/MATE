using Master40.DB.DataModel;
using Master40.DB.Nominal;
using Master40.SimulationCore.Types;

namespace Master40.SimulationCore.Agents.StorageAgent.Behaviour
{
    public static class Factory
    {
        public static IBehaviour Get(M_Stock stockElement, SimulationType simType)
        {
            IBehaviour behaviour;
            switch (simType)
            {
                case SimulationType.Central:
                    behaviour = Central(simType);
                    break;
                default:
                    behaviour = Default(stockElement: stockElement, simType);
                    break;

            }

            return behaviour;
        }

        private static IBehaviour Default(M_Stock stockElement, SimulationType simType)
        {
            return new Default(stockElement: stockElement, simType: simType);

        }
        private static IBehaviour Central(SimulationType simType)
        {
            return new Central(simType);

        }
    }
}
}
