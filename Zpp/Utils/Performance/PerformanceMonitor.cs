using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace Zpp.Util.Performance
{
    public class PerformanceMonitor
    {
        private readonly InstanceToTrack _instanceToTrack;
        private bool _isStarted = false;
        private ulong _start = 0;
        private ulong _measuredCpuCycles = 0;
        private bool _alreadyUsed = false;

        public PerformanceMonitor(InstanceToTrack instanceToTrack)
        {
            _instanceToTrack = instanceToTrack;
        }

        public bool IsStarted()
        {
            return _isStarted;
        }

        public void Start(bool again = false)
        {
            if (_isStarted)
            {
                throw new MrpRunException(
                    "A PerformanceMonitor cannot be started before it is stopped.");
            }

            if (_alreadyUsed && again)
            {
                throw new MrpRunException(
                    "Are you sure to use the performanceMonitor multiple times ? " +
                    "The call Start() with again=true."
                    );
            }

            _start = NativeMethods.GetThreadCycles();
            _isStarted = true;
        }

        public void Stop()
        {
            if (_isStarted == false)
            {
                throw new MrpRunException(
                    "A PerformanceMonitor cannot be stopped before it is started.");
            }

            ulong end = NativeMethods.GetThreadCycles();
            _measuredCpuCycles += end - _start;
            _isStarted = false;
            _alreadyUsed = true;
        }

        public override string ToString()
        {
            string instanceToTrack = Enum.GetName(typeof(InstanceToTrack), _instanceToTrack);
            string objectAsString = $"\"{instanceToTrack}\": \"{_measuredCpuCycles.ToString()}\",";
            return objectAsString;
        }

        public override bool Equals(object obj)
        {
            PerformanceMonitor other = (PerformanceMonitor) obj;
            return _instanceToTrack.Equals(other._instanceToTrack);
        }

        public override int GetHashCode()
        {
            return _instanceToTrack.GetHashCode();
        }

        /**
         * Black magic: https://stackoverflow.com/a/26619409
         */
        private static class NativeMethods
        {
            public static ulong GetThreadCycles()
            {
                ulong cycles;
                if (!QueryThreadCycleTime(PseudoHandle, out cycles))
                    throw new System.ComponentModel.Win32Exception();
                return cycles;
            }

            [DllImport("kernel32.dll", SetLastError = true)]
            private static extern bool QueryThreadCycleTime(IntPtr hThread, out ulong cycles);

            private static readonly IntPtr PseudoHandle = (IntPtr) (-2);
        }

        public ulong GetMeasuredCpuCycles()
        {
            return _measuredCpuCycles;
        }
    }
}