using Microsoft.EntityFrameworkCore.Query.Internal;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mate.Production.Core.Agents.HubAgent.Types.Queuing
{
    internal class JobTracker 
    {
        //private List<IJob> _jobs { get; set; } = new List<IJob>();
        private Dictionary<string, List<IJob>> JobDict { get; set; } = new Dictionary<string, List<IJob>>();

        StreamWriter fs = File.AppendText("C:\\temp\\queuelog_latest.csv");
        StreamWriter fsl = File.AppendText("C:\\temp\\queueLength_log_latest.csv");


        private List<IJob> GetJobs(IJob job)
        {
            JobDict.TryGetValue(job.RequiredCapability.Name, out List<IJob> jobs);
            jobs ??= new List<IJob>();
            return jobs;
        }

        private void SetJobs(List<IJob> jobs, string name)
        {
            JobDict.Remove(name);
            JobDict.Add(name, jobs);
        }

        private void WriteChangeSet(DateTime time, string operation, List<IJob> _jobs, string name) 
        {
            var sorted = _jobs.Select((x, i) => new Tuple<IJob, int>(x, i))
                     .GroupBy(x => (x.Item1.StartCondition.Satisfied.ToString() + x.Item1.RequiredCapability.Id))
                     .OrderByDescending(x => x.Key)
                     .ThenBy(x => x.Min(y => y.Item1.Priority(time)))
                     .SelectMany(x => x)
                     .Select((x, i) => new Tuple<int, IJob, int>(i, x.Item1, Math.Abs(x.Item2 - i)));

            //  var sorted = _jobs
            //      .Select((x, i) => new KeyValuePair<IJob, int>(x, i))
            //      .OrderByDescending(x => x.Key.StartConditions).ThenBy(x => x.Key.RequiredCapability).ThenBy(x => x.Key.Priority(time))
            //      //.OrderBy(x => x.Key.Priority(time))
            //      .Select((x, i) => new Tuple<IJob, int, int>(x.Key, i - x.Value, i))
            //      .ToList();


            _jobs = sorted.Select(x => x.Item2).ToList();
            SetJobs(_jobs, name);

            var output = name + ";" + _jobs.Count + ";" + operation + ";" ;
            foreach (var d in sorted)
                output = string.Concat(output, d.Item3, ";");
            fs.WriteLine(output);


            var outputL = time + ";";
            var qlength = 0;
            foreach (var keyValuePair in JobDict) {
                qlength += keyValuePair.Value.Count;
            }
            qlength /= JobDict.Values.Count;
            outputL = string.Concat(outputL, qlength, ";");
            fsl.WriteLine(outputL);
            // System.Console.WriteLine("---- distance");
            //foreach (var d in sorted)
            //     System.Console.WriteLine("Original Pos " + d.Item3 + " Distance " + d.Item1 + " Value " + d.Item2.Prio);

        }

        public void Add(IJob job, DateTime time)
        {
            var _jobs = GetJobs(job);
            _jobs.RemoveAll(x => x.Key.Equals(job.Key));
            _jobs.Add(job);

            WriteChangeSet(time, "Add", _jobs, job.RequiredCapability.Name);
        }

        public void Remove(IJob job, DateTime time)
        {
            var _jobs = GetJobs(job);
            _jobs.RemoveAll(x => x.Key.Equals(job.Key));
            WriteChangeSet(time, "Remove", _jobs, job.RequiredCapability.Name);
        }

        public void TrackJobs(DateTime time) => WriteChangeSet(time, "Sort", null, null);


    }
}
