using System;
using System.Collections.Generic;

namespace Master40.DB.GanttplanDB.Models
{
    public partial class ObjecttypeUdf
    {
        public string ObjecttypeId { get; set; }
        public string UdfId { get; set; }
        public string Name { get; set; }
        public string Info1 { get; set; }
        public string Info2 { get; set; }
        public string Info3 { get; set; }
        public string Category { get; set; }
        public string Range { get; set; }
        public long? Datatype { get; set; }
    }
}
