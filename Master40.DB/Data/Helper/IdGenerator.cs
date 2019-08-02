using System;
using Master40.DB.Data.WrappersForPrimitives;

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

        public static Id GetRandomId(int minValue, int maxValue)
        {
            return new Id(new Random().Next(minValue, maxValue));
        }
    }
}