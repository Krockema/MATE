using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Master40.SimulationCore.Helper
{
    public class LogWriter
    {
        private readonly string _path;
        private readonly bool _writeToFile;
        /// <summary>
        /// Extend this one to logObject with time, agent, and message
        /// </summary>
        public  List<string> Log { get; set; }
        public LogWriter(bool writeToFile)
        {
            _writeToFile = writeToFile;
            Log = new List<string>();
            _path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var logFileInfo = new FileInfo(_path + "\\" + "log.txt");
            System.Diagnostics.Debug.WriteLine($"Path to  logfile  {_path} logging is : {_writeToFile}");

            if (logFileInfo.Exists)
                logFileInfo.Delete();
            logFileInfo.Create();
        }

        public Task<bool> WriteToFile()
        {
           return  Task.Run(() =>
            {
                if (_writeToFile)
                {
                    File.AppendAllLines(path: _path + "\\" + "log.txt", Log);
                    Log.Clear();
                }
                return true;
            });
        }
    }
}
