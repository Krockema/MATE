using System;
using System.Collections.Generic;
using System.Text;

namespace Master40.Tools.ExtensionMethods
{
    public static class Negate
    {
        public static bool IsFalse(bool negate)
        {
            return !negate;
        }

        public static bool NotEqual<T>(this T negate, T compareTo)
        {
            return IsFalse(negate.Equals(compareTo));
        }
    }
}
