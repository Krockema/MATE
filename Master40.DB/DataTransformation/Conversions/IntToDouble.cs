using System;

namespace Master40.DB.DataTransformation.Conversions
{
    public class IntToDouble : Conversion
    {
        public static new object Convert(string[] args, object inputData, bool reversed = false)
        {
            Type expType = reversed == false ? typeof(int) : typeof(double);
            if (inputData.GetType() != expType)
                throw new ArgumentException(String.Format("Input data must be of type '{0}' if reversed is {1}", expType, reversed));

            if (!reversed)
                return System.Convert.ToDouble(inputData);
            else
                return System.Convert.ToInt32(inputData);
        }
    }
}
