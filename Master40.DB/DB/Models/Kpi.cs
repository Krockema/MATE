using System;
using System.Collections.Generic;
using System.Text;

namespace Master40.DB.Models
{
    public class Kpi : BaseEntity
    {
        public double Value { get; set; }
        public string Name { get; set; }
        public bool IsKpi { get; set; }

    }
}
