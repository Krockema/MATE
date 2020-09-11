using System;

namespace Master40.DB.Util
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
    }
}