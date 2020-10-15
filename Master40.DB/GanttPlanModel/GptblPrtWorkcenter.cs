namespace Master40.DB.GanttPlanModel
{
    public partial class GptblPrtWorkcenter
    {
        public string ClientId { get; set; }
        public string PrtId { get; set; }
        public string WorkcenterId { get; set; }
        public double? FactorSetupTime { get; set; }
        public int? PriorityValue { get; set; }
    }
}
