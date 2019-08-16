using Master40.DB.DataModel;
using Master40.DB.Enums;
using Master40.SimulationCore.Types;

namespace Master40.SimulationCore.Agents.StorageAgent.Behaviour
{
    public static class Factory
    {
        public static IBehaviour Get(M_Stock stockElement, SimulationType simType)
        {
            switch (simType)
            {
                default:
                    return Default(stockElement: stockElement);
            }
        }

        private static IBehaviour Default(M_Stock stockElement)
        {
            return new Default(stockElement: stockElement, simType: SimulationType.None);

        }
    }
}
