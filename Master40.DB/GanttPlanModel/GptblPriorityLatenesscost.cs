namespace Master40.DB.GanttPlanModel
{
    public partial class GptblPriorityLatenesscost
    {
        public string ClientId { get; set; }
        public string PriorityId { get; set; }
        public string Name { get; set; }
        public int Start { get; set; }
        public double? CostsAbsolute { get; set; }
        public int CostsAbsoluteInterval { get; set; }
        public double? CostsRelative { get; set; }
        public int? CostsRelativeInterval { get; set; }
    }
}
