using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace Master40.DB.DataTransformation.Conversions
{
    public class RelativeTimeIntToDateString : Conversion
    {
        // No args defined
        public static new object Convert(string[] args, object inputData, bool reversed = false)
        {
            Type expType = reversed == false ? typeof(int) : typeof(string);
            if (inputData.GetType() != expType)
                throw new ArgumentException(String.Format("Input data must be of type '{0}' if reversed is {1}", expType, reversed));

            string DateFormat = "yyyyMMdd HH:mm:ss";
            DateTime baseDate = new DateTime(2000, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);

            if (!reversed)
            {
                DateTime date = baseDate.AddSeconds((int)inputData);
                return date.ToString(DateFormat);
            }
            else
            {
                DateTime date = DateTime.ParseExact((string)inputData, DateFormat, CultureInfo.InvariantCulture);
                TimeSpan seconds = date.Subtract(baseDate);
                return System.Convert.ToInt32(seconds.TotalSeconds);
            }
        }
    }
}
