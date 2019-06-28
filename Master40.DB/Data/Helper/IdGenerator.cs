namespace Master40.DB.Data.Helper
{
    public class IdGenerator
    {
        private const int _start = 10000;
        private int _currentId = _start;

        public int GetNewId()
        {
            lock (this)
            {
                _currentId++;
                return _currentId;
            }
        }
    }
}