namespace Zpp.WrappersForPrimitives
{
    public class Priority
    {
        private readonly int _priority;

        public Priority(int priority)
        {
            _priority = priority;
        }

        public int GetValue()
        {
            return _priority;
        }
    }
}