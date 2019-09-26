using System;

namespace Master40.DB.Data.WrappersForPrimitives
{
    public class DecimalPrimitive<T>: INumericPrimitive<T> where T : DecimalPrimitive<T>, new()
    {
        private decimal _decimal;

        public DecimalPrimitive(decimal @decimal)
        {
            _decimal = @decimal;
        }

        public DecimalPrimitive()
        {
        }

        public decimal GetValue()
        {
            return _decimal;
        }

        public void IncrementBy(T t)
        {
            _decimal += t._decimal;
        }

        public void IncrementBy(decimal compareOperation)
        {
            _decimal += compareOperation;
        }
        
        public void DecrementBy(T t)
        {
            _decimal -= t._decimal;
        }


        public bool IsGreaterThanOrEqualTo(T t)
        {
            return _decimal >= t._decimal;
        }
        
        public bool IsGreaterThan(T t)
        {
            return _decimal > t._decimal;
        }
        
        public bool IsGreaterThanNull()
        {
            return IsGreaterThan(Null());
        }

        public bool IsSmallerThan(T t)
        {
            return _decimal < t.GetValue();
        }
        
        public bool IsSmallerThanOrEqualTo(T t)
        {
            return _decimal <= t.GetValue();
        }
        
        public T Minus(T t)
        {
            decimal newValue = _decimal - t._decimal;
            T newObject = (T) Activator.CreateInstance(typeof(T));
            newObject._decimal = newValue;
            return newObject;
        }
        
        public T Plus(T t)
        {
            decimal newValue = _decimal + t._decimal;
            T newObject = (T) Activator.CreateInstance(typeof(T));
            newObject._decimal = newValue;
            return newObject;
        }

        public T AbsoluteValue()
        {
            decimal newValue = Math.Abs(_decimal);
            T newObject = (T) Activator.CreateInstance(typeof(T));
            newObject._decimal = newValue;
            return newObject;
        }
        
        public bool IsNull()
        {
            return _decimal.Equals(0);
        }

        public bool IsNegative()
        {
            return _decimal < 0;
        }
        
        public static T Null()
        {
            decimal newValue = 0;
            T newObject = (T) Activator.CreateInstance(typeof(T));
            newObject._decimal = newValue;
            return newObject;
        }
        
        public int CompareTo(T that)
        {
            return _decimal.CompareTo(that.GetValue());
        }

        public int CompareTo(object obj)
        {
            T other = (T)obj;
            return _decimal.CompareTo(other.GetValue());
        }
        
        public override bool Equals(object obj)
        {
            T other = (T) obj;
            return _decimal.Equals(other.GetValue());
        }

        public override int GetHashCode()
        {
            return _decimal.GetHashCode();
        }

        public override string ToString()
        {
            return $"{_decimal}";
        }
    }
}