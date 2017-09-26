using System;
using System.Collections.Generic;
using System.Text;

namespace Master40.DB.Data.Helper
{
    public class BoxPlot
    {
        public string Name { get; set; }
        public decimal LowestSample { get; set; }
        public decimal LowerQuartile { get;  set; }
        public decimal Median { get; set; }
        public decimal UpperQartile { get; set; }
        public decimal HeigestSample { get; set; }
        public string Color { get; set; }

    }
}
