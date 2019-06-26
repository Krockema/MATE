namespace Zpp.Test.WrappersForPrimitives
{
    /**
     * wraps a directory name string
     */
    public class DirectoryName
    {
        private string _direcoryName;

        public DirectoryName(string direcoryName)
        {
            _direcoryName = direcoryName;
        }

        public string GetDirectoryName()
        {
            return _direcoryName;
        }
    }
}