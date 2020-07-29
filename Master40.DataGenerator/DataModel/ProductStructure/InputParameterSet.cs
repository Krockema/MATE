using System;
using System.Collections.Generic;
using System.Text;

namespace Master40.DataGenerator.Model.ProductStructure
{
    public class InputParameterSet
    {
        public int EndProductCount { get; set; }
        public int DepthOfAssembly { get; set; }
        public double ReutilisationRatio { get; set; }
        public double ComplexityRatio { get; set; }
        public double MeanIncomingMaterialAmount { get; set; }
        public double VarianceIncomingMaterialAmount { get; set; }
    }
}
