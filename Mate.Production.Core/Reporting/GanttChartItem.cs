namespace Mate.Production.Core.Reporting
{
    public class GanttChartItem
    {
        public string article { get; set; }
        public string articleId { get; set; }
        public string operation { get; set; }
        public string operationId { get; set; }
        public string resource { get; set; }
        /// <summary>
        /// integer formated start
        /// </summary>
        public string start { get; set; }
        /// <summary>
        /// integer formated end
        /// </summary>
        public string end { get; set; }
        /// <summary>
        /// Color giving identifier
        /// </summary>
        public string groupId { get; set; } 
        public string priority { get; set; }
        public string IsProcessing { get; set; }
        public string IsReady { get; set; }
        public string IsFinalized { get; set; }
        public string IsWorking { get; set; }
        public string IsFixed { get; set; }
    }
}
