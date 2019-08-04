using System.Collections.Generic;
using Zpp.DemandDomain;

namespace Zpp.MachineDomain
{
    public interface IMachineManager
    {
        /**
         * Giffler-Thomson
         */
        IMachine GetNextFreeMachine(ProductionOrderBoms productionOrderBoms);
    }
}