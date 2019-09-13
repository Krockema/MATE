namespace Zpp.Test.WrappersForPrimitives
{
    /**
     * wraps a line string
     */
    public class Line
    {
        private string _line;

        public Line(string line)
        {
            _line = line;
        }

        public string GetValue()
        {
            return _line;
        }
    }
}