using System;
using Akka.Actor;
using Mate.DataCore.DataModel;

public interface IJob
{
    Guid Key { get; }
    string Name { get; }
    DateTime ForwardStart { get; }
    DateTime ForwardEnd { get; }
    DateTime BackwardStart { get; }
    DateTime BackwardEnd { get; }
    DateTime Start { get; }
    DateTime End { get; }
    StartConditionRecord StartConditions { get; }
    Func<DateTime, double> Priority { get; }
    IActorRef HubAgent { get; }
    DateTime DueTime { get; }
    TimeSpan Duration { get; }
    int SetupKey { get; }
    M_ResourceCapability RequiredCapability { get; }
    Func<DateTime, IJob> UpdateEstimations { get; }
    string Bucket { get; }
    Func<string, IJob> UpdateBucket { get; }
    Action ResetSetup { get; }
}
