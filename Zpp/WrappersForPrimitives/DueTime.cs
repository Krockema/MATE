using System;

namespace ZppForPrimitives
{
    public class DueTime : IComparable<DueTime>
    {
        public int _dueTime;

        public DueTime(int dueTime)
        {
            _dueTime = dueTime;
        }

        public int GetDueTime()
        {
            return _dueTime;
        }
        
        public int CompareTo(DueTime that)
        {
            if (_dueTime >  that.GetDueTime()) return -1;
            if (_dueTime == that.GetDueTime()) return 0;
            return 1;
        }
    }
}