using System;
namespace Mate.Production.Core.Environment.Records.Reporting
{
    public record CreateTaskItemRecord(
        string Type,
        string Resource,
        int ResourceId,
        DateTime Start,
        DateTime End,
        string Capability,
        string Operation,
        long GroupId
    );
}