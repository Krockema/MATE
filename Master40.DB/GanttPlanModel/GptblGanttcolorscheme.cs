using System;

namespace Master40.DB.GanttPlanModel
{
    public partial class GptblGanttcolorscheme
    {
        public string ClientId { get; set; }
        public string GanttcolorschemeId { get; set; }
        public string Name { get; set; }
        public string Category { get; set; }
        public int? DefaultType { get; set; }
        public int? DefaultColor { get; set; }
        public string DefaultGanttcolorschemeId { get; set; }
        public string DefaultRandomcolorPropertyId { get; set; }
        public string UserId { get; set; }
        public DateTime? LastModified { get; set; }
    }
}
