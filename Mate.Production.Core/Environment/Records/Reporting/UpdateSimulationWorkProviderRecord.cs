using System.Collections.Immutable;

namespace Mate.Production.Core.Environment.Records.Reporting
{
    public record UpdateSimulationWorkProviderRecord(

        IImmutableSet<StockProviderRecord> ArticleProviderRecords,
        string RequestAgentId,
        string RequestAgentName,
        bool IsHeadDemand,
        int CustomerOrderId 
    );
}
