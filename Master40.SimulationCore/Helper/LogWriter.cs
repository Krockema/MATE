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
        private StreamWriter writer;
        public  List<string> Log { get; set; }
        public LogWriter()
        {
            Log = new List<string>();
            _path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var logFileInfo = new FileInfo(_path + "\\" + "log.txt");
            System.Diagnostics.Debug.WriteLine($"Path to  logfile  {_path}");

            if (logFileInfo.Exists)
                logFileInfo.Delete();
            logFileInfo.Create();
        }

        public Task<bool> WriteToFile()
        {
           return  Task.Run(() =>
            {
                File.AppendAllLines(path: _path + "\\" + "log.txt", Log);
                Log.Clear();
                return true;
            });
        }
    }
}
