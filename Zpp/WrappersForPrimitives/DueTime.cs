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

        public override bool Equals(object obj)
        {
            DueTime otherDueTime = (DueTime) obj;
            return _dueTime.Equals(otherDueTime._dueTime);
        }

        public override int GetHashCode()
        {
            return _dueTime.GetHashCode();
        }
    }
}