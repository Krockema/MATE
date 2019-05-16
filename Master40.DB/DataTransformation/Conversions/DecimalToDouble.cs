using System;

namespace Master40.DB.DataTransformation.Conversions
{
    public class DecimalToDouble : Conversion
    {
        // No args defined
        public static new object Convert(string[] args, object inputData, bool reversed = false)
        {
            Type expType = reversed == false ? typeof(decimal) : typeof(double);
            if (inputData.GetType() != expType)
                throw new ArgumentException(String.Format("Input data must be of type '{0}' if reversed is {1}", expType, reversed));

            if(!reversed)
                return decimal.ToDouble((decimal)inputData);
            else
                return (decimal)(double)inputData;
        }
    }
}
