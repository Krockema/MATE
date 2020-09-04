using Master40.DB.DataModel;
using Master40.DB.Nominal;
using Master40.SimulationCore.Types;
using static FCentralStockDefinitions;

namespace Master40.SimulationCore.Agents.StorageAgent.Behaviour
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
