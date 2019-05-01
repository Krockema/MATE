using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace Master40.DB.DataTransformation.Conversions
{
    public class Conversion
    {
        public static object Convert(string[] args, object inputData, bool reversed = false)
        {
            throw new NotImplementedException();
        }

        public static object DoConvert(string convName, string convArgs, object inputData, bool reversed = false)
        {
            if (convName == null)
                return inputData;
            string nameSpace = MethodBase.GetCurrentMethod().DeclaringType.Namespace;
            Type convType = Type.GetType(nameSpace + "." + convName);
            if (convType == null)
                throw new NotImplementedException(String.Format("Conversion class with name '{0}' is not defined", convName));
            // convArgs are seperated by ';' and will be casted in Convert-method 
            string[] args = null;
            if(convArgs != null)
                args = convArgs.Split(";");
            return convType.GetMethod("Convert").Invoke(null, new object[] { args, inputData, reversed });
        }
    }

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
