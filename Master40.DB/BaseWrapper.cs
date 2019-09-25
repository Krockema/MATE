namespace Master40.DB
{
    /// <summary>
    /// Maybe the wrong place..
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class BaseWrapper<T>
    {
        private readonly T _value;
        public BaseWrapper(T value)
        {
            _value = value;
        }
        
        public T Value => _value;
    }
}
