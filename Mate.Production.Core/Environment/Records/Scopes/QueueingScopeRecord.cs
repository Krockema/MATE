using Mate.Production.Core.Environment.Records.Interfaces;

namespace Mate.Production.Core.Environment.Records.Scopes
{
    public record QueueingScopeRecord(bool IsQueueAble, bool IsRequieringSetup, ITimeRange Scope);
} 
