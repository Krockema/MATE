using System;
using System.Collections.Generic;
using System.Text;

namespace Master40.DB.Enums
{
    public enum KpiType
    {
        LeadTime,
        MachineUtilization,
        Timeliness,
        StockEvolution,
        LayTime,
        AgentStatistics,
        MeanTimeToStart,
        PheromoneHistory //von Malte: neuer Pheromone-KPI Typ
    }
}
