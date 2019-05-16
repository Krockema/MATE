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
}
