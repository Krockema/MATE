using System;

namespace Zpp.Test.WrappersForPrimitives
{
    /**
     * wraps a lineNumber
     */
    public class LineNumber
    {
        private int _lineNumber;

        public LineNumber(int lineNumber)
        {
            _lineNumber = lineNumber;
        }

        public int GetLineNumber()
        {
            return _lineNumber;
        }

        public override string ToString()
        {
            return (_lineNumber + 1).ToString();
        }
    }
}