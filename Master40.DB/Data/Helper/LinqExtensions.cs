using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Master40.DB.Data.Helper
{
    public static class LinqExtensions
    {
        public static void CopyPropertiesTo<T>(this T source, T dest)
        {
            var plist = from prop in typeof(T).GetProperties() where prop.CanRead && prop.CanWrite select prop;

            foreach (PropertyInfo prop in plist)
            {
                if (prop.Name != "Id")
                    prop.SetValue(dest, prop.GetValue(source, null), null);
            }
        }
    }
}
