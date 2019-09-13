namespace Zpp.Test.WrappersForPrimitives
{
    /**
     * wraps a file name string
     */
    public class FileName
    {
        private string _fileName;

        public FileName(string fileName)
        {
            _fileName = fileName;
        }

        public string GetValue()
        {
            return _fileName;
        }

        public override string ToString()
        {
            return _fileName;
        }
    }
}