using System;
using System.Collections.Generic;
using System.Text;
using Master40.DB.DB.Models;

namespace Master40.DB.DB.Interfaces
{
    public interface ISimulationProductionOrderWorkSchedule
    {
        int HierarchyNumber { get; set; }
        int Start { get; set; }
        int End { get; set; }
        int ProductionOrderId { get; set; }
        ProductionOrder ProductionOrder { get; set; }
    }
}
