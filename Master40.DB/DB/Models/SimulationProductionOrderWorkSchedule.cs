using Master40.DB.DB.Interfaces;
using Master40.DB.Models;
using Master40.Simulation.Simulation;

namespace Master40.DB.DB.Models
{
    public class SimulationProductionOrderWorkSchedule : BaseEntity, ISimulationProductionOrderWorkSchedule
    {
        public SimulationProductionOrderWorkSchedule()
        {
            SimulatedStart = 0;
            SimulatedEnd = 0;
            SimulatedDuration = 0;
        }
        public int HierarchyNumber { get; set; }
        public int Start { get; set; }
        public int End { get; set; }
        public int ProductionOrderId { get; set; }
        public ProductionOrder ProductionOrder { get; set; }
        public SimulationState SimulationState { get;set;}
        public int SimulatedStart { get; set; }
        public int SimulatedEnd { get; set; }
        public int SimulatedDuration { get; set; }
        public int SimulationId { get; set; }
    }

    public enum SimulationState
    {
        Waiting,
        InProgress,
        Finished
    }
}
