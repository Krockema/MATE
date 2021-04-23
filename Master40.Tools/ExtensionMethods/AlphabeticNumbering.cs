using System;

namespace Master40.Tools.ExtensionMethods
{
    public class AlphabeticNumbering
    {
        public static string GetAlphabeticNumbering(int number)
        {
            return number == 0 ? "A" : AppendNextLetter("", number);
        }

        private static string AppendNextLetter(string subResult, int remainingPortion)
        {
            if (remainingPortion == 0)
            {
                return subResult;
            }
            var remainder = remainingPortion % 26;
            var c = (char) (65 + remainder);
            return AppendNextLetter(c + subResult, remainingPortion / 26);
        }

        public static int GetNumericRepresentation(string number)
        {
            return number == "A" ? 0 : AddNextExponent(0, number);
        }

        private static int AddNextExponent(int exponent, string remainingPortion)
        {
            if (remainingPortion == "")
            {
                return 0;
            }

            char lastLetter = remainingPortion[remainingPortion.Length - 1];
            var value = (lastLetter - 65) * (int) Math.Pow(26, exponent);
            remainingPortion = remainingPortion.Substring(0, remainingPortion.Length - 1);
            return value + AddNextExponent(exponent + 1, remainingPortion);
        }
    }
}