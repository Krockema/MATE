using Master40.DB.DataModel;
using Master40.DB.Enums;
using Master40.SimulationCore.MessageTypes;
using System;
using System.Collections.Generic;
using System.Text;

namespace Master40.SimulationCore.Agents.StorageAgent.Behaviour
{
    public static class BehaviourFactory
    {
        public static IBehaviour Get(M_Stock stockElement, SimulationType simType)
        {
            switch (simType)
            {
                default:
                    return Default(stockElement);
            }
        }

        private static IBehaviour Default(M_Stock stockElement)
        {
            return new Default(stockElement, SimulationType.None);

        }
    }
}
