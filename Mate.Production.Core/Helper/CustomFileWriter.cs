using System;
using System.IO;
using System.Threading;

namespace Mate.Production.Core.Helper
{
    public static class CustomFileWriter
    {
        static ReaderWriterLock rwl = new ReaderWriterLock();

        public static void WriteToFile(string file, string text)
        {
            try
            {
                rwl.AcquireWriterLock(500);
                try
                {
                    File.AppendAllText($"{System.Environment.CurrentDirectory}//{file}", text);
                }
                finally
                {
                    // Ensure that the lock is released.
                    rwl.ReleaseWriterLock();
                }
            }
            catch (ApplicationException)
            {
            }
        }
    }
}
