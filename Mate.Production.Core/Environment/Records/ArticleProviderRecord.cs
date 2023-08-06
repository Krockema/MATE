using System;
using System.Collections.Immutable;

namespace Mate.Production.Core.Environment.Records
{
    public record ArticleProviderRecord(
        Guid ArticleKey
        , string ArticleName
        , Guid StockExchangeId
        , DateTime ArticleFinishedAt
        , DateTime CustomerDue
        , IImmutableSet<StockProviderRecord> Provider);
}