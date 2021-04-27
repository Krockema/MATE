namespace Mate.DataCore.Nominal.Model
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