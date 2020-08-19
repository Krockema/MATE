using System.Collections.Generic;

namespace Master40.DataGenerator.DataModel.ProductStructure
{
    public class ProductStructure
    {
        public List<Dictionary<long, Node>> NodesPerLevel { get; set; }
        public List<Edge> Edges { get; set; }

        public ProductStructure()
        {
            NodesPerLevel = new List<Dictionary<long, Node>>();
            Edges = new List<Edge>();
        }

    }
}
