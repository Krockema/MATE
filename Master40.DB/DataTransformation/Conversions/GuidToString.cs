using System;
using System.Collections.Generic;
using System.Text;

namespace Master40.DB.DataTransformation.Conversions
{
    public class GuidToString : Conversion
    {
        // No args defined
        public static new object Convert(string[] args, object inputData, bool reversed = false)
        {
            Type expType = reversed == false ? typeof(Guid) : typeof(string);
            if (inputData.GetType() != expType)
                throw new ArgumentException(String.Format("Input data must be of type '{0}' if reversed is {1}", expType, reversed));

            if (!reversed)
                return ((Guid)inputData).ToString();
            else
                return Guid.Parse((string)inputData);
        }
    }
}
