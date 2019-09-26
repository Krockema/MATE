using System;

namespace Master40.DB.Data.WrappersForPrimitives
{
    public interface INumericPrimitive<T>: IComparable<T>,IComparable 
    {
        void IncrementBy(T t);

        void DecrementBy(T t);


        bool IsGreaterThanOrEqualTo(T t);
        
        bool IsGreaterThan(T t);
        
        bool IsGreaterThanNull();

        bool IsSmallerThan(T t);
        
        bool IsSmallerThanOrEqualTo(T t);
        
        T Minus(T t);
        
        T Plus(T t);

        T AbsoluteValue();
        
        bool IsNull();

        bool IsNegative();
    }
}