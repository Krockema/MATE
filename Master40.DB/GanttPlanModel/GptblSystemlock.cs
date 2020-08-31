using System;

namespace Master40.DB.GanttPlanModel
{
    public partial class GptblSystemlock
    {
        public string ClientId { get; set; }
        public string ObjectId { get; set; }
        public string ObjecttypeId { get; set; }
        public DateTime Date { get; set; }
        public string Host { get; set; }
        public int? ProcessId { get; set; }
        public string UserId { get; set; }
        public string ProductioncontrollerIds { get; set; }
    }
}
