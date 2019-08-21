using System.Collections.Generic;
using Newtonsoft.Json;

namespace Zpp.GraphicalRepresentation
{
    public class GanttChart : IGanttChart
    {
        private readonly List<GanttChartBar> _ganttChartBars = new List<GanttChartBar>();
        
        public void AddGanttChartBar(GanttChartBar ganttChartBar)
        {
            _ganttChartBars.Add(ganttChartBar);
        }

        public List<GanttChartBar> GetAllGanttChartBars()
        {
            return _ganttChartBars;
        }

        public override string ToString()
        {
            return JsonConvert.SerializeObject(_ganttChartBars, Formatting.Indented);
        }
    }
}