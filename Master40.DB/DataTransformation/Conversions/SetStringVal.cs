using System;
using System.Collections.Generic;
using System.Text;

namespace Master40.DB.DataTransformation.Conversions
{
    class SetStringVal : Conversion
    {
        // Arg          Type        Name        Default
        // ================================================
        // args[0]      string      value       0
        public static new object Convert(string[] args, object inputData, bool reversed = false)
        {
            if (reversed == true)
                throw new ArgumentException("This conversion function cannot be reversed!");

            // Resolve args
            string value = "";
            if (args != null)
                value = args[0];

            return value;
        }
    }
}
