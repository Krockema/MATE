using System;
using System.Collections.Generic;

namespace Master40.DB.GanttplanDB.Models
{
    public partial class Shift
    {
        public string ShiftId { get; set; }
        public string Name { get; set; }
        public string Info1 { get; set; }
        public string Info2 { get; set; }
        public string Info3 { get; set; }
        public long? BgRed { get; set; }
        public long? BgGreen { get; set; }
        public long? BgBlue { get; set; }
    }
}
