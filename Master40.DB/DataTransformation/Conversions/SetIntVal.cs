using System;
using System.Collections.Generic;
using System.Text;

namespace Master40.DB.DataTransformation.Conversions
{
    public class SetIntVal : Conversion
    {
        // Arg          Type        Name        Default
        // ================================================
        // args[0]      int        value       0
        public static new object Convert(string[] args, object inputData, bool reversed = false)
        {
            if (reversed == true)
                throw new ArgumentException("This conversion function cannot be reversed!");

            // Resolve args
            long value = 0;
            if (args != null)
                value = int.Parse(args[0]);

            return value;
        }
    }
}
