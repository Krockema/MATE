namespace Master40.DB.GanttPlanModel
{
    public partial class GptblConfirmationResource
    {
        public string ClientId { get; set; }
        public string ConfirmationId { get; set; }
        public string ResourceId { get; set; }
        public int ResourceType { get; set; }
        public string GroupId { get; set; }
        public int? Allocation { get; set; }
    }
}
