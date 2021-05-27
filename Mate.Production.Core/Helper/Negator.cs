namespace Mate.Production.Core.Helper
{
    public static class Negate
    {
        public static bool IsFalse(bool negate)
        {
            return !negate;
        }
        public static bool IsNot(bool negate)
        {
            return !negate;
        }
        public static bool NotEqual<T>(this T negate, T compareTo)
        {
            return IsFalse(negate.Equals(compareTo));
        }
        public static bool IsNull<T>(this T obj) => obj == null;
        
        public static bool IsNotNull<T>(this T obj) => obj != null;
        
    }
}
