﻿using System;
using System.Collections.Generic;
using System.Linq;
using Akka.Actor;
using Mate.DataCore.DataModel;
using MathNet.Numerics.Statistics;
using static FBuckets;
using static FOperations;
using static IConfirmations;
using static IJobs;

namespace Mate.Production.Core.Agents.ResourceAgent.Types
{
    public class JobInProgress
    {
        private IConfirmation Current { get; set; }
        public CurrentOperation CurrentOperation { get; set; } = new CurrentOperation();
        private SortedList<long, IConfirmation> ReadyElements { get; } = new SortedList<long, IConfirmation>();
        public long ResourceIsBusyUntil => (ReadyElements.Count > 0) ? ReadyElements.Values.Max(x => x.ScopeConfirmation.GetScopeEnd()) 
                                                                     : ExpectedResourceIsBusyUntil;
        private long ExpectedResourceIsBusyUntil { get; set; }
        public long StartedAt { get; private set; }
        public bool IsSet => Current != null;
        public bool SetupIsOngoing { get; set; }
        public long LastTimeStartCall { get; private set; }
        public bool IsWorking {get; private set; } = false;
        private Queue<FOperation> _finalOperations { get; set; } = new Queue<FOperation>();
        public bool ResetIsWorking() => IsWorking = false;
        public int HasCurrent=> Current != null ? 1 : 0;

        #region Passthrough Properties
        public long JobDuration => Current.Job.Duration;
        public long JobMaxDuration => ((FBucket) Current.Job).MaxBucketSize;
        public int ResourceCapabilityId => Current.CapabilityProvider.ResourceCapabilityId;
        public int CapabilityProviderId => Current.CapabilityProvider.Id;
        public string RequiredCapabilityName => Current.Job.RequiredCapability.Name;
        public string JobName => Current.Job.Name;
        public Guid JobKey => Current.Job.Key;
        public IActorRef JobAgentRef => Current.JobAgentRef;
        public IConfirmation GanttItem => Current;
        public List<IConfirmation> GanttItems => this.ReadyElements.Values.ToList();
        public bool IsCurrentDelayed(long currentTime) => Current.ScopeConfirmation.GetScopeStart() < currentTime;
        public M_ResourceCapabilityProvider CapabilityProvider => Current.CapabilityProvider;
        public int GetTotalOperationsOfJobInProgress()
        {
            int operations = 0;
            int durationOperations = 0;
            if (ReadyElements.Count > 0)
            {
                foreach (var element in ReadyElements)
                {
                    operations += ((FBucket)element.Value.Job).Operations.Count();
                }
            }
            if(HasCurrent == 1)
            {
                operations += ((FBucket)Current.Job).Operations.Count();
            }

            return operations;
        }

        public (int, int) GetTotalOperationsOfJobInProgress(Guid operationId, long currentTime)
        {
            int operations = 0;
            int durationOperations = 0;
            IJob operation = null;

            if (HasCurrent == 1)
                operation = ((FBucket)Current.Job).Operations.SingleOrDefault(x => x.Key.Equals(operationId)) as IJob;
            
            //Wenn es in dem aktuellen Element ist
            if (operation != null)
            {
                var jobPrio = operation.Priority(currentTime);
                operations += ((FBucket)Current.Job).Operations.Count(x => ((IJob)x).Priority(currentTime) < jobPrio);
                durationOperations += ((FBucket)Current.Job).Operations.Where(x => ((IJob)x).Priority(currentTime) < jobPrio).Sum(y => y.Operation.Duration);
            }
            //Ansonsten, wenn es in den ReadyElementen ist
            else
            {
                if (HasCurrent == 1)
                {
                    operations += ((FBucket)Current.Job).Operations.Count();
                }
                IJob element = null;
                foreach (var item in ReadyElements)
                {
                    //Gehe solange über die Liste, bis du das Element in den Operationen gefunden hast
                    while (element == null)
                    {
                        var tmp = item.Value.Job as FBucket;
                        element = tmp.Operations.SingleOrDefault(x => x.Key.Equals(operationId));
                        //((FBucket)ReadyElements.ToArray()[i].Job).Operations.SingleOrDefault(x => x.Key.Equals(operationId));

                        //Wenn das Element noch nicht in der Liste ist, nimm alle Operationen aus dieser Liste
                        if (element == null)
                        {
                            operations += tmp.Operations.Count;
                            break;
                        }

                        var jobPrio = element.Priority(currentTime);
                        operations += tmp.Operations.Count(x => ((IJob)x).Priority(currentTime) < jobPrio);
                        durationOperations += tmp.Operations.Where(x => ((IJob)x).Priority(currentTime) < jobPrio).Sum(y => y.Operation.Duration);

                    }
                }
            }
            return (durationOperations, operations);
        }



        #endregion

        public void StartProcessing(long currentTime, long duration)
        {
            LastTimeStartCall = currentTime;
            if (IsWorking)
                return;

            ExpectedResourceIsBusyUntil = currentTime + duration;
            StartedAt = currentTime;
            IsWorking = true;
        }

        public void StartSetup(long currentTime, long duration)
        {
            LastTimeStartCall = currentTime;
            var max = (new long[] {Current.ScopeConfirmation.GetScopeEnd(), currentTime + duration}).Max();
            ExpectedResourceIsBusyUntil = max; //Current.Duration;
            StartedAt = currentTime;
            IsWorking = true;
        }



        public bool Set(IConfirmation jobConfirmation, long duration)
        {
            if (IsSet)
                return false;
            Current = jobConfirmation;
            StartedAt = jobConfirmation.ScopeConfirmation.GetScopeStart();
            ExpectedResourceIsBusyUntil = jobConfirmation.ScopeConfirmation.GetScopeEnd();
            return true;
        }
        /// <summary>
        /// returns true if the update was successful
        /// </summary>
        /// <param name="jobConfirmation"></param>
        /// <returns></returns>
        public bool UpdateJob(IConfirmation jobConfirmation)
        {
            
            if(IsSet && Current.Key.Equals(jobConfirmation.Key))
            {
                Current = Current.UpdateJob(jobConfirmation.Job);
                //ExpectedResourceIsBusyUntil = jobConfirmation.ScopeConfirmation.GetScopeEnd();
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
            ExpectedResourceIsBusyUntil = 0;
            _finalOperations = new ();
        }

        public (int, IConfirmation) RevokeJob(Guid confirmationKey)
        {
            if (IsSet && Current.Key.Equals(confirmationKey))
            {
                var rev = Current;
                Reset();
                return (0, rev);
            }

            var (key, revoked) = ReadyElements.FirstOrDefault(x => x.Value.Key.Equals(confirmationKey));
            if (revoked == null) return (0, null);
            ReadyElements.Remove(key);
            return (JobPosition(revoked), revoked);
        }

        public int JobPosition(IConfirmation job)
        {
            var position = 0;
            if (IsSet) 
                position = position + ((FBucket)Current.Job).Operations.Count();
            position = position + ReadyElements.IndexOfKey(job.ScopeConfirmation.GetScopeStart());
            return position;
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

        public void DissolveBucketToQueue(long currentTime)
        {
            if (_finalOperations.Count > 0)
            {
                throw new Exception("Previous work was not done.");
            }
            _finalOperations = new Queue<FOperation>(((FBucket)Current.Job).Operations.OrderByDescending(prio => prio.Priority.Invoke(currentTime)));
        }
        public FOperation DequeueNextOperation()
        {
            return _finalOperations.Dequeue();
        }

        public FOperation[] OperationsAsArray()
        {
            return _finalOperations.Any() ? _finalOperations.ToArray() 
                                            : ((FBucket) Current.Job).Operations.ToArray();
        }
    }
}
