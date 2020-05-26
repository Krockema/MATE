using static IConfirmations;

namespace Master40.SimulationCore.Agents.ResourceAgent.Types
{
    public class JobInProgress
    {
        public IConfirmation Current { get; private set; }
        public long ResourceIsBusyUntil { get; private set; } = 0;
        public long StartedAt { get; private set; }
        public bool IsSet => Current != null;
        public bool IsWorking {get; private set; } = false;
        public bool IsFinalized { get; private set; } = false;
        public int SetupId => Current.CapabilityProvider.Id;
        public string RequiredCapabilityName => Current.Job.RequiredCapability.Name;

        public void Start(long currentTime, long busyUntil)
        {
            if (IsWorking)
                return;
            
            StartedAt = currentTime;
            ResourceIsBusyUntil = busyUntil;
            IsWorking = true;
        }
        public bool Set(IConfirmation jobConfirmation, long busyUntil)
        {
            if (IsSet)
                return false;
            Current = jobConfirmation;
            StartedAt = jobConfirmation.ScopeConfirmation.GetScopeStart();
            ResourceIsBusyUntil = busyUntil;
            return true;
        }
        public bool UpdateJobs(IConfirmation jobConfirmation, long busyUntil)
        {
            Current = Current.UpdateJob(jobConfirmation.Job);
            ResourceIsBusyUntil = busyUntil;
            IsFinalized = true;
            return true;
        }


        public void Reset()
        {
            Current = null;
            IsWorking = false;
            IsFinalized = false;
            StartedAt = 0;
            ResourceIsBusyUntil = 0;
            
        }
    }
}
