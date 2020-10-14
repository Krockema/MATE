namespace Master40.DB.GanttPlanModel
{
    public partial class GptblRoutingScrapItem
    {
        public string ClientId { get; set; }
        public string RoutingId { get; set; }
        public double QuantityLimit { get; set; }
        public double? ScrapRate { get; set; }
    }
}
