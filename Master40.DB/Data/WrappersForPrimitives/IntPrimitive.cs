using System;

namespace Master40.DB.Data.WrappersForPrimitives
{
    public class IntPrimitive<T> : INumericPrimitive<T> where T : IntPrimitive<T>, new()
    {
        private int _int;

        public IntPrimitive(int @int)
        {
            _int = @int;
        }

        public IntPrimitive()
        {
        }

        public int GetValue()
        {
            return _int;
        }

        public void IncrementBy(T t)
        {
            _int += t._int;
        }

        public void DecrementBy(T t)
        {
            _int -= t._int;
        }


        public bool IsGreaterThanOrEqualTo(T t)
        {
            return _int >= t._int;
        }

        public bool IsGreaterThan(T t)
        {
            return _int > t._int;
        }

        public bool IsGreaterThanNull()
        {
            return IsGreaterThan(Null());
        }

        public bool IsSmallerThan(T t)
        {
            return _int < t._int;
        }

        public bool IsSmallerThanOrEqualTo(T t)
        {
            return _int <= t._int;
        }

        public T Minus(T t)
        {
            int newValue = _int - t._int;
            T newObject = (T) Activator.CreateInstance(typeof(T));
            newObject._int = newValue;
            return newObject;
        }

        public T Plus(T t)
        {
            int newValue = _int + t._int;
            T newObject = (T) Activator.CreateInstance(typeof(T));
            newObject._int = newValue;
            return newObject;
        }

        public T AbsoluteValue()
        {
            int newValue = Math.Abs(_int);
            T newObject = (T) Activator.CreateInstance(typeof(T));
            newObject._int = newValue;
            return newObject;
        }

        public bool IsNull()
        {
            return _int.Equals(0);
        }

        public bool IsNegative()
        {
            return _int < 0;
        }

        public static T Null()
        {
            int newValue = 0;
            T newObject = (T) Activator.CreateInstance(typeof(T));
            newObject._int = newValue;
            return newObject;
        }

        public int CompareTo(T that)
        {
            return _int.CompareTo(that._int);
        }

        public int CompareTo(object obj)
        {
            T other = (T) obj;
            return _int.CompareTo(other._int);
        }

        public override bool Equals(object obj)
        {
            T other = (T) obj;
            if (other == null)
            {
                return false;
            }

            return _int.Equals(other._int);
        }

        public override int GetHashCode()
        {
            return _int.GetHashCode();
        }

        public override string ToString()
        {
            return $"{_int}";
        }
    }
}