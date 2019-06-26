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