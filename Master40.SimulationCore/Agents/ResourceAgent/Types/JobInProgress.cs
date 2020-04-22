using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using static FBuckets;
using static FJobConfirmations;
using static FOperations;
using static IConfirmations;
using static IJobs;

namespace Master40.SimulationCore.Agents.ResourceAgent.Types
{
    public class JobInProgress
    {
        public IConfirmation Current { get; private set; }
        public long StartTime { get; private set; }
        public long ResourceIsBusyUntil { get; set; } = 0;
        public bool IsSet => Current != null;
        public bool IsWorking {get; set; } = false;
        public void SetStartTime(long time) => StartTime = time;
        public int SetupId => Current.CapabilityProvider.Id;
        public string RequiredCapabilityName => Current.Job.RequiredCapability.Name;

        public bool Set(IConfirmation jobConfirmation, long currentTime)
        {
            if (IsSet)
                return false;
            Current = jobConfirmation;
            ResourceIsBusyUntil = jobConfirmation.ScopeConfirmation.GetScopeEnd();
            StartTime = currentTime;
            return true;
        }

        public void Reset()
        {
            Current = null;
            IsWorking = false;
            ResourceIsBusyUntil = 0;
            
        }
    }
}
