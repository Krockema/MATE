using System;
using System.Collections.Generic;
using System.Linq;

namespace Master40.Models
{
    public class HangfireJob
    {
        public static string PROCESSING = "processing";
        public static string SUCCEEDED = "succeeded";
        public static string FAILED = "failed";
        public static string QUEUED = "queued";
        
        public HangfireJob()
        {
        }

        public string DataItem => getChartMatrix();
        public int MaxY { get; set; }
        public int MaxX { get; set; }
       
        // Tuple< x , y, state>
        private List<Tuple<int, int, string>> _matrix = new List<Tuple<int, int, string>>();

        public void AddJob(dynamic item, string state)
        {
            try
            {
                var y = item.Job.Args[0]; // SimulationsId
                var x = item.Job.Args[1]; // Simulationsnummer
                _matrix.Add(new Tuple<int, int, string>(x, y, state));
                this.MaxX = Math.Max(x, this.MaxX);
                this.MaxY = Math.Max(y, this.MaxY);
            }
            catch
            {
            }
        }

        private string getChartMatrix()
        {
            var matrixString = "";
            _matrix.OrderByDescending(d => d.Item2).ToList()
                .ForEach(x => matrixString += "{ x: " + x.Item1 + ", y: " + (MaxY + 1 - x.Item2) + ", v: '" + x.Item3 + "' },");
            return matrixString;
        }

        private List<Tuple<int, int, string>> getAllRunning()
        {
            return _matrix.FindAll(x => x.Item3 == "processing");
        }
    }
}
