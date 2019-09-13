namespace Zpp.GraphicalRepresentation
{
    public class GanttChartBar
    {
        // TODO: remove or use unused properties
        
        // not used by gantt chart app
        public string article { get; set; }
        
        // not used by gantt chart app
        public string articleId { get; set; }
        public string operation { get; set; }
        
        // not used by gantt chart app
        public string operationId { get; set; }
        public string resource { get; set; }
        public string start { get; set; }
        public string end { get; set; }

        public string groupId { get; set; } = "1";
    }
}