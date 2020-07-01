using System.Collections.Generic;
using Zpp.GraphicalRepresentation.impl;

namespace Zpp.GraphicalRepresentation
{
    public interface IGanttChart
    {
        void AddGanttChartBar(GanttChartBar ganttChartBar);

        List<GanttChartBar> GetAllGanttChartBars();
    }
}