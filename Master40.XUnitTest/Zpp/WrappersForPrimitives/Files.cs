using System.Collections.Generic;
using System.IO;

namespace Master40.XUnitTest.Zpp.WrappersForPrimitives
{
    /**
     * wraps all Files as FileInfo
     */
    public class Files
    {
        private List<FileInfo> _files;

        public Files(List<FileInfo> files)
        {
            _files = files;
        }
        
    }
}