using System;
using Microsoft.EntityFrameworkCore.Query.Internal;
using Microsoft.Extensions.DependencyModel;

namespace Master40.DB.Data.WrappersForPrimitives
{
    public class DecimalPrimitive<T>: INumericPrimitive<T> where T : DecimalPrimitive<T>, new()
    {
        private decimal _decimal;
        private decimal _remainder;

        public DecimalPrimitive(decimal @decimal)
        {
            _decimal = @decimal;
            _remainder = 0;
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
        
        public bool IsGreaterThanZero()
        {
            return IsGreaterThan(Zero());
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

        /// <summary>
        /// Decrements the value to Zero and returns true if Zero is exceeded.
        /// </summary>
        /// <param name="q"></param>
        /// <returns></returns>
        public bool MinusToZero(T t)
        {
            if (this._decimal >= t._decimal)
            {
                DecrementBy(t);
                _remainder = 0;
                return false;
            }
            else
            {
                _remainder = t._decimal - this._decimal;
                _decimal = 0;
                return true;
            }
        }

        public bool IsNull()
        {
            if (this._decimal == 0)
            {
                return true;
            }
            else return false;
        }

        public T GetRemainder()
        {
            T newObject = (T)Activator.CreateInstance(typeof(T));
            newObject._decimal = _remainder;
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
        
        public bool IsZero()
        {
            return _decimal.Equals(0);
        }

        public bool IsNegative()
        {
            return _decimal < 0;
        }
        
        public static T Zero()
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