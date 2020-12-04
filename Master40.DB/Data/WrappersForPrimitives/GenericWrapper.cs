using SQLitePCL;

namespace Master40.DB.Data.WrappersForPrimitives
{
    /// <summary>
    /// generic wrapper for Primitives to become reference able in dictionaries
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class GenericWrapper<T>
    {
        private T _value;

        public GenericWrapper(T value)
        {
            _value = value;
        }

        public T GetValue()
        {
            return _value;
        }

        public void SetValue(T value)
        {
            _value = value;
        }
    }
}