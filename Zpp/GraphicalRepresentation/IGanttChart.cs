using System.Collections.Generic;

namespace Zpp.GraphicalRepresentation
{
    public interface IGanttChart
    {
        void AddGanttChartBar(GanttChartBar ganttChartBar);

        List<GanttChartBar> GetAllGanttChartBars();
    }
}