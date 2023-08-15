﻿using Akka.Actor;
using Akka.Hive.Definitions;
using Mate.DataCore.DataModel;
using Mate.Production.Core.Environment.Records.Interfaces;
using System;

public record OperationRecord(
          Guid Key,
          DateTime DueTime,
          DateTime CreationTime,
          DateTime BackwardEnd,
          DateTime BackwardStart,
          DateTime ForwardEnd,
          DateTime ForwardStart,
          TimeSpan RemainingWork,
          DateTime CustomerDue,
          DateTime Start,
          DateTime End,
          StartConditionRecord StartCondition,
          Func<DateTime, double> Priority,
          IActorRef ProductionAgent,
          Guid ArticleKey,
          IActorRef HubAgent,
          bool IsFinished,
          M_Operation Operation,
          M_ResourceCapability RequiredCapability,
          int SetupKey,
          string Bucket
          ) : IKey, IJob, IComparable
{
    public Guid Key { get; set; }
    public StartConditionRecord StartConditions { get; set; }
    public Guid ArticleKey { get; set; }
    public IActorRef HubAgent { get; set; }
    public bool IsFinished { get; set; }
    public int SetupKey { get; set; }

    int IComparable.CompareTo(object obj)
    {
        if (obj is OperationRecord other)
        {
            return Key.CompareTo(other.Key);
        }
        throw new ArgumentException("Cannot compare value of different types", nameof(obj));
    }

    //bool Equals(object obj)
    //{
    //    if (obj is OperationRecord operation)
    //    {
    //        return Key.Equals(operation.Key);
    //    }
    //    throw new ArgumentException("Cannot compare value of different types", nameof(obj));
    //}
 
    public override int GetHashCode()
    {
        return Key.GetHashCode();
    }

    public void SetFinished()
    {
        IsFinished = true;
    }
    public IJob AsJob => this;

    //IJob UpdateEstimations = 
    public Action ResetSetup => (() => SetupKey = -1);

    public Func<DateTime, IJob> UpdateEstimations => ((estimatedStart) => this with { End = estimatedStart + Operation.Duration, Start = estimatedStart });
    Func<string, IJob> IJob.UpdateBucket => ((bucketId) => this with { Bucket = bucketId });
    public string Name => this.Name;
    Func<DateTime, double> IJob.Priority => time => Priority(time);
    public TimeSpan Duration => this.Duration;
    public OperationRecord SetForwardSchedule(DateTime earliestStart) => this with { ForwardStart = earliestStart, ForwardEnd = earliestStart + Operation.Duration };
    public OperationRecord UpdateCustomerDue(DateTime due) => this with { CustomerDue = due };
    public OperationRecord UpdateBucket(string bucketId) => this with { Bucket = bucketId };
    public void UpdateHubAgent(IActorRef hub)  => this.HubAgent = hub;
    public void SetStartConditions(bool preCondition, bool articleProvided, DateTime time)
    {
        StartConditions = new StartConditionRecord
        (
            PreCondition :  preCondition,
            ArticlesProvided : articleProvided,
            WasSetReadyAt : (preCondition && articleProvided && StartConditions.WasSetReadyAt == Time.ZERO.Value)
                ? time
                : StartConditions.WasSetReadyAt
        );
    }
}