using Master40.DB.Data.Helper.Types;
using Master40.DB.Data.WrappersForPrimitives;
using Master40.SimulationMrp.impl;

namespace Master40.SimulationMrp
{
    public interface IZppSimulator
    {
        void StartOneCycle(SimulationInterval simulationInterval, Quantity customerOrderQuantity);

        void StartOneCycle(SimulationInterval simulationInterval);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="shouldPersist">Should dbTransactionData and dbTransactionDataArchive
        /// be persisted at the end</param>
        void StartPerformanceStudy(bool shouldPersist);

        void StartTestCycle(bool shouldPersist=true);

        void StartMultipleTestCycles();
    }
}