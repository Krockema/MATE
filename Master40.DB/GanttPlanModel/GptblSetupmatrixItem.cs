namespace Master40.DB.GanttPlanModel
{
    public partial class GptblSetupmatrixItem
    {
        public string ClientId { get; set; }
        public string SetupmatrixId { get; set; }
        public string FromOptimizationgroupId { get; set; }
        public string ToOptimizationgroupId { get; set; }
        public int? SetupTime { get; set; }
        public double? SetupCosts { get; set; }
        public string Comment { get; set; }
    }
}
