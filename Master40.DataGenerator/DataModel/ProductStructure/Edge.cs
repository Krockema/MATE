using System;
using System.Collections.Generic;
using System.Text;

namespace Master40.DataGenerator.DataModel.ProductStructure
{
    public class Edge
    {
        public Node Start { get; set; }
        public Node End { get; set; }
        public double Weight { get; set; }
    }
}
