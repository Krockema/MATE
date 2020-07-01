using Master40.DB.Data.Initializer;

namespace Master40.DB.Nominal.Model
{
    public class Setup
    {
        public ModelSize Value { get; }

        public Setup(ModelSize size)
        {
            Value = size;
        }

    }
}