using Mate.DataCore.DataModel;

namespace Mate.DataCore.Interfaces
{
    public interface ISimulationProductionOrderWorkSchedule
    {
        int HierarchyNumber { get; set; }
        int Start { get; set; }
        int End { get; set; }
        int? ProductionOrderId { get; set; }
        T_ProductionOrder ProductionOrder { get; set; }
    }
}
