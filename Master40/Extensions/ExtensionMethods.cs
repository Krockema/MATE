using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Master40.Extensions
{
    public static class ExtensionMethods
    {
        public static void CopyPropertiesTo<T>(this T source, T dest)
        {
            var plist = from prop in typeof(T).GetProperties() where prop.CanRead && prop.CanWrite select prop;

            foreach (PropertyInfo prop in plist)
            {
                prop.SetValue(dest, prop.GetValue(source, null), null);
            }
        }
        public static IEnumerable<Color> GetGradients(Color start, Color end, int steps)
        {
            int stepA = ((end.A - start.A) / (steps - 1));
            int stepR = ((end.R - start.R) / (steps - 1));
            int stepG = ((end.G - start.G) / (steps - 1));
            int stepB = ((end.B - start.B) / (steps - 1));

            var colorList = new List<Color>();
            for (int i = 0; i < steps; i++)
            {
                colorList.Add(new Color
                {
                    A = start.A + (stepA * i),
                    R = start.R + (stepR * i),
                    G = start.G + (stepG * i),
                    B = start.B + (stepB * i)
                });
            }
            return colorList;
        }
        
        public class Color 
        {
            public int A { get; set; }
            public int R { get; set; }
            public int G { get; set; }
            public int B { get; set; }
        }
    
        public static int GetEpochSeconds(this DateTime date)
        {
            TimeSpan t = DateTime.UtcNow - new DateTime(1970, 1, 1);
            return (int)t.TotalSeconds;
        }
    }
}
