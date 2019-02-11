using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Text;

namespace Master40.SimulationCore.Helper
{
    public static class PropertieExtender
    {
        public static void AddProperty(this ExpandoObject expando, string propertyName, object propertyValue)
        {
            var expandoDict = expando as IDictionary<string, object>;
            if (expandoDict.ContainsKey(propertyName))
                expandoDict[propertyName] = propertyValue;
            else
                expandoDict.Add(propertyName, propertyValue);
        }
    }
}
