using System;

namespace Mate.Production.Core.Environment.Records
{
    public record CreateSimulationJobRecord
        (string Key
        , DateTime DueTime
        , string ArticleName
        , string OperationName
        , int OperationHierarchyNumber
        , TimeSpan OperationDuration
        , string RequiredCapabilityName
        , string JobType
        , string CustomerOrderId
        , bool IsHeadDemand
        , Guid ArticleKey
        , string ArticleNameRecord
        , string ProductionAgent
        , string ArticleType
        , string JobName
        , string CapabilityProvider
        , DateTime Start
        , DateTime End);
}