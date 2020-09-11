using System.Collections.Generic;
using Master40.DB.DataModel;

namespace Master40.DataGenerator.DataModel.ProductStructure
{
    public class Node
    {
        public int AssemblyLevel { get; set; }
        public int? WorkPlanLength { get; set; }
        public M_Article Article { get; set; }
        public List<Edge> IncomingEdges { get; set; } = new List<Edge>();
    }
}
