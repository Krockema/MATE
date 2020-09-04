using Master40.DB.Nominal;
using System;
using static FCentralStockDefinitions;

namespace Master40.SimulationCore.Agents.StorageAgent.Behaviour
{
    class Central : SimulationCore.Types.Behaviour
    {
        private FCentralStockDefinition _stock { get; }
        public Central(FCentralStockDefinition stockDefinition, SimulationType simType) : base(simulationType: simType)
        {
            _stock = stockDefinition;
        }

        public override bool Action(object message)
        {
            switch (message)
            {
                default: return false;
            }
            return true;
        }
    }
}
