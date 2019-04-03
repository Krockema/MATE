using System;
using System.Collections.Generic;
using System.Text;

namespace Master40.DB.Models
{
    public class Mapping : BaseEntity
    {
        public string From { get; set; }
        public string To { get; set; }
        public bool IsAgentData { get; set; }
        public string ConversionFunc { get; set; }
        public string ConversionArgs { get; set; }
    }
}
