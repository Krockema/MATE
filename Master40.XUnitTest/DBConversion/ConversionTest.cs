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

        [Fact]
        public void TestIntToDouble()
        {
            int intVal = 123;
            double expDoubleVal = 123.0;
            double doubleVal = 456.0;
            int expIntVal = 456;
            string multi = "3.0";
            double expDoubleValMulti = 369.0;
            int expIntValMulti = 152;

            double doubleRes = (double)IntToDouble.Convert(null, intVal);
            int intRes = (int)IntToDouble.Convert(null, doubleVal, true);
            double doubleResMulti = (double)IntToDouble.Convert(new string[] { multi }, intVal);
            int intResMulti = (int)IntToDouble.Convert(new string[] { multi }, doubleVal, true);

            Assert.Equal(expDoubleVal, doubleRes);
            Assert.Equal(expIntVal, intRes);
            Assert.Equal(expDoubleValMulti, doubleResMulti);
            Assert.Equal(expIntValMulti, intResMulti);
        }

        [Fact]
        public void TestSetLongVal()
        {
            string value = "123";
            long expLongVal = 123;

            long longRes = (long)SetLongVal.Convert(new string[] { value }, null);

            Assert.Equal(expLongVal, longRes);
        }

        [Fact]
        public void TestRelativeTimeIntToDateString()
        {
            int intTimeVal = 1337;
            string expStringDateVal = "19700101 00:22:17";
            string stringDateVal = "19700101 00:13:17";
            int expIntTimeVal = 797;

            string stringDateRes = (string)RelativeTimeIntToDateString.Convert(null, intTimeVal);
            int intTimeRes = (int)RelativeTimeIntToDateString.Convert(null, stringDateVal, true);

            Assert.Equal(expStringDateVal, stringDateRes);
            Assert.Equal(expIntTimeVal, intTimeRes);
        }

        [Fact]
        public void TestGuidToString()
        {
            Guid guidVal = Guid.Parse("12345678-1234-1234-1234-1234567890ab");
            string expStringVal = "12345678-1234-1234-1234-1234567890ab";
            string stringVal = "12345678-abcd-abcd-abcd-1234567890ab";
            Guid expGuidVal = Guid.Parse("12345678-abcd-abcd-abcd-1234567890ab");

            string stringRes = (string)GuidToString.Convert(null, guidVal);
            Guid guidRes = (Guid)GuidToString.Convert(null, stringVal, true);

            Assert.Equal(expStringVal, stringRes);
            Assert.Equal(expGuidVal, guidRes);
        }
    }
}
