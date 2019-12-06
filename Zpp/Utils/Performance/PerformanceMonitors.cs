using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Zpp.Util.Performance
{
    public class PerformanceMonitors
    {
        private readonly Dictionary<InstanceToTrack, PerformanceMonitor> _monitors =
            new Dictionary<InstanceToTrack, PerformanceMonitor>();

        public PerformanceMonitors()
        {
            
            foreach (InstanceToTrack instanceToTrack in Enum.GetValues(typeof(InstanceToTrack)))
            {
                    _monitors.Add(instanceToTrack, new PerformanceMonitor(instanceToTrack));   
            }
        }

        public void Start(InstanceToTrack instancesToTrack)
        {
            _monitors[instancesToTrack].Start();
        }

        public void Stop(InstanceToTrack instancesToTrack)
        {
            _monitors[instancesToTrack].Stop();
        }

        public override string ToString()
        {
            // create report
            string report = "{" + Environment.NewLine;
            foreach (InstanceToTrack instancesToTrack in Enum.GetValues(typeof(InstanceToTrack)))
            {
                report += _monitors[instancesToTrack].ToString() + Environment.NewLine +
                          Environment.NewLine;
            }
            
            // long currentMemoryUsage = GC.GetTotalMemory(false);
            long currentMemoryUsage = Process.GetCurrentProcess().WorkingSet64;
            report +=
                $"\"CurrentMemoryUsage\": \"{currentMemoryUsage}\"" +
                Environment.NewLine;
            report += "}" + Environment.NewLine;

            return report;
        }
    }
}