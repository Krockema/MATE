namespace Master40.DB.GanttPlanModel
{
    public partial class GptblReportColorfilter
    {
        public string ClientId { get; set; }
        public string ReportId { get; set; }
        public string FilterId { get; set; }
        public int? BgColor { get; set; }
        public int? FilterOperator { get; set; }
        public int? FilterResultCountMax { get; set; }
        public int? FontColor { get; set; }
        public int? FullColumnColoring { get; set; }
        public int? FullRowColoring { get; set; }
        public int? PriorityValue { get; set; }
        public string PropertyId { get; set; }
    }
}
