using System;

namespace Zpp.WrappersForPrimitives
{
    public class DueTime : IComparable<DueTime>,IComparable 
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

        public int CompareTo(object obj)
        {
            DueTime otherDueTime = (DueTime)obj;
            return _dueTime.CompareTo(otherDueTime.GetValue());
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

        public override string ToString()
        {
            return _dueTime.ToString();
        }

        public bool IsNull()
        {
            return _dueTime.Equals(0);
        }

        public DueTime Minus(DueTime dueTime)
        {
            return new DueTime(_dueTime-dueTime.GetValue());
        }
        
        public DueTime Minus(int dueTime)
        {
            return new DueTime(_dueTime-dueTime);
        }
        
        public static DueTime Null()
        {
            return new DueTime(0);
        }

        public bool IsGreaterThan(DueTime other)
        {
            return _dueTime > other._dueTime;
        }
        
        public bool IsGreaterThanOrEqualTo(DueTime other)
        {
            return _dueTime >= other._dueTime;
        }

        public void IncrementBy(DueTime dueTime)
        {
            _dueTime += dueTime._dueTime;
        }
    }
}