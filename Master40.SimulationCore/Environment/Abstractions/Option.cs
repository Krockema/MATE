namespace Master40.SimulationCore.Environment.Abstractions
{
    public abstract class Option<T> : IOption<T>
    {
        protected T _value;
        public T Value => _value;
    }
}
