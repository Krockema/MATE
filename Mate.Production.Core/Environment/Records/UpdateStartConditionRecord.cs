using System;
namespace Mate.Production.Core.Environment.Records
{
    public record UpdateStartConditionRecord(
        Guid OperationKey,
         DateTime CustomerDue,
         bool PreCondition,
         bool ArticlesProvided);
}
