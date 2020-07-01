using Master40.DB.Data.Initializer;

namespace Master40.DB.Nominal.Model
{
    public class Operator
    {
        public ModelSize Value { get; set; }
        public Operator(ModelSize size)
        {
            Value = size;
        }
    }
}