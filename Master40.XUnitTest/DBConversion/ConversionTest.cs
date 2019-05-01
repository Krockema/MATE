using System;
using System.Collections.Generic;
using System.Text;
using Xunit;
using Master40.DB.DataTransformation.Conversions;

namespace Master40.XUnitTest.DBConversion
{
    public class ConversionTest
    {
        [Fact]
        public void TestDoConvert()
        {
            int intVal = 1337;
            string expStrVal = "1337";
            string strVal = "123";
            int expIntVal = 123;

            string strRes = (string) Conversion.DoConvert("IntToString", null, intVal);
            int intRes = (int)Conversion.DoConvert("IntToString", null, strVal, true);

            Assert.Equal(expStrVal, strRes);
            Assert.Equal(expIntVal, intRes);
        }

        [Fact]
        public void TestDoConvertWithArgs()
        {
            bool boolVal = false;
            long expLongVal = 1;
            long longVal = 1;
            bool expBoolVal = true;

            long longRes = (long)Conversion.DoConvert("BoolToLong", "true", boolVal);
            bool boolRes = (bool)Conversion.DoConvert("BoolToLong", "false", longVal, true);

            Assert.Equal(expLongVal, longRes);
            Assert.Equal(expBoolVal, boolRes);
        }

        [Fact]
        public void TestIntToString()
        {
            int intVal = 1337;
            string expStrVal = "1337";
            string strVal = "123";
            int expIntVal = 123;

            string strRes = (string) IntToString.Convert(null, intVal);
            int intRes = (int) IntToString.Convert(null, strVal, true);

            Assert.Equal(expStrVal, strRes);
            Assert.Equal(expIntVal, intRes);
        }

        [Fact]
        public void TestBoolToLong()
        {
            bool boolVal = false;
            long expLongVal = 0;
            long longVal = 1;
            bool expBoolVal = false;

            long longRes = (long)BoolToLong.Convert(new string[] { "false" }, boolVal);
            bool boolRes = (bool)BoolToLong.Convert(new string[] { "true" }, longVal, true);

            Assert.Equal(expLongVal, longRes);
            Assert.Equal(expBoolVal, boolRes);
        }

        [Fact]
        public void TestDecimalToDouble()
        {
            decimal decimalVal = 1.2345m;
            double expDoubleVal = 1.2345;
            double doubleVal = 0.1234;
            decimal expDecimalVal = 0.1234m;

            double doubleRes = (double)DecimalToDouble.Convert(null, decimalVal);
            decimal decimalRes = (decimal)DecimalToDouble.Convert(null, doubleVal, true);

            Assert.Equal(expDoubleVal, doubleRes);
            Assert.Equal(expDecimalVal, decimalRes);
        }
    }
}
