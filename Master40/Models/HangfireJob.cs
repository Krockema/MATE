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


        public bool HasJobs => _matrix.Any();
        public string DataItem => getChartMatrix();
        public int MaxY { get; set; }
        public int MaxX { get; set; }
       
        // Tuple< x , y, state>
        private List<Tuple<int, int, string>> _matrix = new List<Tuple<int, int, string>>();

        public bool AddJob(dynamic item, string state)
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
                return false;
            }
            return true;
        }

        private string getChartMatrix()
        {
            var matrixString = "";
            _matrix.OrderByDescending(d => d.Item2).ToList()
                .ForEach(g => matrixString += "{ x: " + g.Item1 + ", y: " + (MaxY + 1 - g.Item2) + ", v: '" + g.Item3 + "' },");

            for (int i = 1; i < MaxY; i++)
            {
                if (!_matrix.Exists(x => x.Item2 == i))
                {
                    for (int j = 1; j < MaxX + 1; j++)
                    {
                        matrixString += "{ x: " + j + ", y: " + (MaxY + 1 - i) + ", v: 'empty' },";
                    }
                }
            }
            return matrixString;
        }

        private List<Tuple<int, int, string>> getAllRunning()
        {
            return _matrix.FindAll(x => x.Item3 == "processing");
        }
    }
}
