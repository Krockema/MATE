using System.Collections.Generic;

namespace Zpp.Test.WrappersForPrimitives
{
    /**
     * wraps the lines of a file as string
     */
    public class Lines
    {
        private List<Line> _lines;

        public Lines(List<Line> lines)
        {
            _lines = lines;
        }
        
        public Lines(string[] lines)
        {
            _lines = new List<Line>();
            foreach (var line in lines)
            {
                _lines.Add(new Line(line));
            }
        }

        public List<Line> GetLines()
        {
            return _lines;
        }

        public Line GetLine(LineNumber lineNumber)
        {
            return _lines[lineNumber.GetLineNumber()];
        }

        public int Count()
        {
            return _lines.Count;
        }
    }
}