using Master40.DB.Data.Initializer;

namespace Master40.DB.Nominal.Model
{
    public class Resource
    {
        public ModelSize Value { get; set; }
        public Resource(ModelSize size)
        {
            Value = size;
        }
    }
}