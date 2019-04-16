using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace Master40.DB.DataTransformation.Conversions
{
    public class Conversion
    {
        public static object Convert(object inputData, bool reversed = false)
        {
            throw new NotImplementedException();
        }

        public static object DoConvert(string convName, object inputData, bool reversed = false)
        {
            if (convName == null)
                return inputData;
            string nameSpace = MethodBase.GetCurrentMethod().DeclaringType.Namespace;
            Type convType = Type.GetType(nameSpace + "." + convName);
            if (convType == null)
                throw new NotImplementedException(String.Format("Conversion class with name '{0}' is not defined", convName));
            return convType.GetMethod("Convert").Invoke(null, new object[] { inputData, reversed });
        }
    }

    public class IntToString : Conversion
    {
        public static new object Convert(object inputData, bool reversed = false)
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
