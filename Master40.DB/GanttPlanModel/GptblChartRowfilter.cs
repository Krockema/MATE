namespace Master40.DB.GanttPlanModel
{
    public partial class GptblChartRowfilter
    {
        public string ClientId { get; set; }
        public string ChartId { get; set; }
        public string FilterId { get; set; }
        public int? FilterOperator { get; set; }
        public int? PriorityValue { get; set; }
    }
}
