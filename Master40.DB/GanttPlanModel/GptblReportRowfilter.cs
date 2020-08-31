namespace Master40.DB.GanttPlanModel
{
    public partial class GptblReportRowfilter
    {
        public string ClientId { get; set; }
        public string ReportId { get; set; }
        public string FilterId { get; set; }
        public int? FilterOperator { get; set; }
        public int? FilterResultCountMax { get; set; }
        public int? PriorityValue { get; set; }
    }
}
