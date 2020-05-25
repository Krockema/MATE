using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using Newtonsoft.Json;

namespace Master40.SimulationCore.Helper
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
                    File.AppendAllTextAsync($"{System.Environment.CurrentDirectory}//{file}", text);
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
