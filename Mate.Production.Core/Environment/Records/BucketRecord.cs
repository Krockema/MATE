using System;
using System.Collections.Immutable;
using System.Linq;
using Akka.Actor;
using Akka.Hive.Definitions;
using Mate.DataCore.DataModel;
using Mate.Production.Core.Environment.Records.Interfaces;
using Mate.Production.Core.Helper;

namespace Mate.Production.Core.Environment.Records
{
    public record BucketRecord(string Name,
                            bool IsFixPlanned,
                            DateTime CreationTime,
                            DateTime BackwardEnd,
                            DateTime BackwardStart,
                            DateTime ForwardEnd,
                            DateTime ForwardStart,
                            TimeSpan Scope,
                            DateTime Start,
                            DateTime End,
                            StartConditionRecord StartConditions,
                            Func<BucketRecord, DateTime, double> Priority,
                            IActorRef HubAgent,
                            ImmutableHashSet<OperationRecord> Operations,
                            M_ResourceCapability RequiredCapability,
                            int SetupKey,
                            TimeSpan MaxBucketSize,
                            long MinBucketSize,
                            string Bucket) : IKey, IJob
    {
        public Guid Key { get; }
        public int SetupKey { get; set; }
        public IActorRef HubAgent { get; set; }
        public StartConditionRecord StartConditions { get; set; }
        public BucketRecord SetStartConditions(bool preCondition, bool articleProvided, DateTime time)
        {
            StartConditions = new StartConditionRecord
           (
                    PreCondition: preCondition,
                    ArticlesProvided: articleProvided,
                    WasSetReadyAt: (preCondition && articleProvided && StartConditions.WasSetReadyAt == Time.ZERO.Value) ? time : Time.ZERO.Value
            );
            return this;
        }

        public DateTime DueTime => Operations.Min(y => y.DueTime);
        Func<DateTime, double> IJob.Priority => time => Priority(this, time);
        public TimeSpan Duration => TimeSpan.FromMinutes(Operations.Sum(y => y.Operation.Duration.TotalMinutes));
        public Func<DateTime, IJob> UpdateEstimations => estimatedStart => 
            this with { End = estimatedStart + TimeSpan.FromMinutes(Operations.Sum(y => y.Operation.Duration.TotalMinutes)),
                           Start = estimatedStart };
        Func<string, IJob> IJob.UpdateBucket => (bucketId) => 
            this with { Bucket = bucketId };
        Action IJob.ResetSetup => () => SetupKey = -1;

        public IJob UpdateBucket(string bucketId) => 
            this with { Bucket = bucketId };
        // Other methods
        public BucketRecord UpdateHubAgent(IActorRef hub) => 
            this with { HubAgent = hub };

        public BucketRecord AddOperation(OperationRecord op) => 
            this with { Operations = Operations.Add(op) };

        public BucketRecord RemoveOperation(OperationRecord op) =>  
            this with { Operations = Operations.Remove(op) };

        public BucketRecord SetFixPlanned() => 
            this with { IsFixPlanned = true };

        public TimeSpan GetCapacityLeft() 
            => (BackwardStart - ForwardStart) - Operations.Sum(y => y.Operation.Duration);
        
        public bool HasSatisfiedJob() 
            => Operations.Any(y => y.StartConditions.Satisfied);
        

    }
}
