using System.Reflection;
using System.Text;

namespace Master40.DataGenerator.DataModel.ProductStructure
{
    public class InputParameterSet
    {
        private PropertyInfo[] _propertyInfos = null;
        public int EndProductCount { get; set; }
        public int DepthOfAssembly { get; set; }
        public double ReutilisationRatio { get; set; }
        public double ComplexityRatio { get; set; }
        public double MeanIncomingMaterialAmount { get; set; }
        public double VarianceIncomingMaterialAmount { get; set; }
        public double? MeanWorkPlanLength { get; set; }
        public double? VarianceWorkPlanLength { get; set; }

        public override string ToString()
        {
            var sb = new StringBuilder("[" + base.GetType().ToString() + "]" + "\n");
            _propertyInfos ??= this.GetType().GetProperties();

            foreach (var info in _propertyInfos)
            {
                var value = info.GetValue(this, null) ?? "(null)";
                sb.AppendLine("\t" + info.Name + ": " + value.ToString());
            }

            sb.AppendLine("");
            return sb.ToString();
        }
    }

}
