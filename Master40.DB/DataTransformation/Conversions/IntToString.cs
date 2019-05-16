using System;

namespace Master40.DB.DataTransformation.Conversions
{
    public class IntToString : Conversion
    {
        // No args defined
        public static new object Convert(string[] args, object inputData, bool reversed = false)
        {
            Type expType = reversed == false ? typeof(int) : typeof(string);
            if (inputData.GetType() != expType)
                throw new ArgumentException(String.Format("Input data must be of type '{0}' if reversed is {1}", expType, reversed));

            if (!reversed)
                return inputData.ToString();
            else
                return Int32.Parse((string)inputData);
        }
    }
}
