using System;
using System.Globalization;

namespace Master40.DB.DataTransformation.Conversions
{
    public class IntToDouble : Conversion
    {
        // Arg          Type        Name        Default
        // ================================================
        // args[0]      double      multiplier  1
        public static new object Convert(string[] args, object inputData, bool reversed = false)
        {
            Type expType = reversed == false ? typeof(int) : typeof(double);
            if (inputData.GetType() != expType)
                throw new ArgumentException(String.Format("Input data must be of type '{0}' if reversed is {1}", expType, reversed));

            // Resolve args
            double multiplier = 1.0;
            if (args != null)
                multiplier = double.Parse(args[0], CultureInfo.InvariantCulture);

            if (!reversed)
                return System.Convert.ToDouble(inputData) * multiplier;
            else
                return System.Convert.ToInt32((double)inputData / multiplier);
        }
    }
}
