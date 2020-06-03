using System;
using System.Collections.Generic;
using System.Data.HashFunction.xxHash;
using System.Linq;
using Akka.Actor;
using Master40.DB.DataModel;
using NLog;
using static IConfirmations;

namespace Master40.SimulationCore.Agents.ResourceAgent.Types
{
    public class JobInProgress
    {
        private IConfirmation Current { get; set; }
        private SortedList<long, IConfirmation> ReadyElements { get; } = new SortedList<long, IConfirmation>();
        public long ResourceIsBusyUntil => (ReadyElements.Count > 0) ? ReadyElements.Values.Max(x => x.ScopeConfirmation.GetScopeEnd()) 
                                                                     :  (StartedAt + ExpectedDuration);
        private long ExpectedDuration { get; set; }
        public long StartedAt { get; private set; }
        public bool IsSet => Current != null;
        public bool IsWorking {get; private set; } = false;
        
    #region Passthrough Properties
        public long JobDuration => Current.Job.Duration;
        public int ResourceCapabilityId => Current.CapabilityProvider.ResourceCapabilityId;
        public int CapabilityProviderId => Current.CapabilityProvider.Id;
        public string RequiredCapabilityName => Current.Job.RequiredCapability.Name;
        public string JobName => Current.Job.Name;
        public Guid JobKey => Current.Job.Key;
        public IActorRef JobAgentRef => Current.JobAgentRef;
        public IConfirmation GanttItem => Current;
        public bool IsCurrentDelayed(long currentTime) => Current.ScopeConfirmation.GetScopeStart() < currentTime;
        public M_ResourceCapabilityProvider CapabilityProvider => Current.CapabilityProvider;
        #endregion

        public void Start(long currentTime, long duration)
        {
            if (IsWorking)
                return;
            
            StartedAt = currentTime;
            ExpectedDuration = Current.Duration;
            IsWorking = true;
        }
        public bool Set(IConfirmation jobConfirmation, long duration)
        {
            if (IsSet)
                return false;
            Current = jobConfirmation;
            StartedAt = jobConfirmation.ScopeConfirmation.GetScopeStart();
            ExpectedDuration = duration;
            return true;
        }
        public bool UpdateJob(IConfirmation jobConfirmation)
        {
            
            if(IsSet && Current.Key.Equals(jobConfirmation.Key))
            {
                Current = Current.UpdateJob(jobConfirmation.Job);
                ExpectedDuration = Current.Duration;
                return true;
            }

            var (key, ready) = ReadyElements.FirstOrDefault(x => x.Value.Key.Equals(jobConfirmation.Key));
            if (ready == null) return false;
            ReadyElements.Remove(key);
            ReadyElements.Add(key ,ready.UpdateJob(jobConfirmation.Job));
            return true;
        }
        public void Reset()
        {
            Current = null;
            IsWorking = false;
            StartedAt = 0;
            ExpectedDuration = 0;
        }

        public IConfirmation RevokeJob(Guid confirmationKey)
        {
            if (IsSet && Current.Key.Equals(confirmationKey))
            {
                var rev = Current;
                Reset();
                return rev;
            }

            var (key, revoked) = ReadyElements.FirstOrDefault(x => x.Value.Key.Equals(confirmationKey));
            if (revoked == null) return null;
            ReadyElements.Remove(key);
            return revoked;
        }

        public void Add(IConfirmation confirmation)
        {
            ReadyElements.Add(confirmation.ScopeConfirmation.GetScopeStart(), confirmation);
        }

        public IConfirmation ReadyItemToProcessingItem()
        {
            if(IsSet) return null;
            var (key, first) = ReadyElements.FirstOrDefault();
            if (first != null)
            {
                ReadyElements.Remove(key);
                Set(first, first.ScopeConfirmation.GetScopeEnd() - first.ScopeConfirmation.GetScopeStart());
            }
            return first;
        }
    }
}
