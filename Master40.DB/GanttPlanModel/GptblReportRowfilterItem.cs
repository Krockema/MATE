namespace Master40.DB.GanttPlanModel
{
    public partial class GptblReportRowfilterItem
    {
        public string ClientId { get; set; }
        public string ReportId { get; set; }
        public string FilterId { get; set; }
        public int? CaseSensitive { get; set; }
        public int? FilterMatchingType { get; set; }
        public string PropertyId { get; set; }
        public string Value { get; set; }
    }
}
