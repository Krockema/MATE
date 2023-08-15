using System;

public class CreateResourceSetupRecord
{
    public DateTime Start { get; init; }
    public TimeSpan Duration { get; init; }
    public string CapabilityProvider { get; init; } = string.Empty;
    public string CapabilityName { get; init; } = string.Empty;
    public TimeSpan ExpectedDuration { get; init; }
    public int SetupId { get; init; }
}

