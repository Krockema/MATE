using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace Master40.DataGenerator.Model.ProductStructure
{
    public class ProductStructure
    {
        public List<List<Node>> NodesPerLevel { get; set; }
        public List<Edge> Edges { get; set; }

        public ProductStructure()
        {
            NodesPerLevel = new List<List<Node>>();
            Edges = new List<Edge>();
        }


    }
}
