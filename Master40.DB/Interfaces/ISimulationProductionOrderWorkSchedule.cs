using System;
using System.Collections.Generic;
using System.Text;
using Master40.DB.DataModel;

namespace Master40.DB.Interfaces
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
