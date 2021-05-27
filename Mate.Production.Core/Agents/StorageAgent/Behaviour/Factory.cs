using Mate.DataCore.DataModel;
using Mate.DataCore.Nominal;
using Mate.Production.Core.Types;
using static FCentralStockDefinitions;

namespace Mate.Production.Core.Agents.StorageAgent.Behaviour
{
    public static class Factory
    {
        public static IBehaviour Get(M_Stock stockElement, SimulationType simType)
        {
            IBehaviour behaviour;
            switch (simType)
            {
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
        public static IBehaviour Central(FCentralStockDefinition stockDefinition, SimulationType simType)
        {
            return new Central(stockDefinition, simType);
        }
    }
}
