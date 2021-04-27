namespace Mate.DataCore.Nominal.Model
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