using System;

namespace Zpp.WrappersForPrimitives
{
    public class DueTime : IComparable<DueTime>
    {
        private int _dueTime;

        public DueTime(int dueTime)
        {
            _dueTime = dueTime;
        }

        public int GetValue()
        {
            return _dueTime;
        }
        
        public int CompareTo(DueTime that)
        {
            return _dueTime.CompareTo(that.GetValue());

        }
    }
}