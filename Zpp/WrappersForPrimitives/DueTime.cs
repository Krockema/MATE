using System;

namespace Zpp.WrappersForPrimitives
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
            if (this._dueTime >  that.Balance) return -1;
            if (this.Balance == that.Balance) return 0;
            return 1;
        }
    }
}