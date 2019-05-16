using System;

namespace Master40.DB.DataTransformation.Conversions
{
    public class BoolToLong : Conversion
    {
        // Arg          Type        Name        Default
        // ================================================
        // args[0]      bool        inverted    false
        public static new object Convert(string[] args, object inputData, bool reversed = false)
        {
            Type expType = reversed == false ? typeof(bool) : typeof(long);
            if (inputData.GetType() != expType)
                throw new ArgumentException(String.Format("Input data must be of type '{0}' if reversed is {1}", expType, reversed));

            // Resolve args
            bool inverted = false;
            if (args != null)
                inverted = args[0] == "true";

            if (!reversed)
            {
                if (!inverted)
                    return (long)((bool)inputData ? 1 : 0);
                else
                    return (long)((bool)inputData ? 0 : 1);
            }
            else
            {
                if (!inverted)
                    return (long)inputData != 0;
                else
                    return (long)inputData == 0;
            }
        }
    }
}
