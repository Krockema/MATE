using System;

namespace Master40.DB.GanttPlanModel
{
    public partial class GptblShift
    {
        public string ClientId { get; set; }
        public string ShiftId { get; set; }
        public string Info1 { get; set; }
        public string Info2 { get; set; }
        public string Info3 { get; set; }
        public string Name { get; set; }
        public byte BgBlue { get; set; }
        public byte BgGreen { get; set; }
        public byte BgRed { get; set; }
        public DateTime? LastModified { get; set; }
    }
}
