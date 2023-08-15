using System;

namespace Mate.Production.Core.Environment.Records
{
    public record SetEstimatedThroughputTimeRecord
    (
        int ArticleId,
        TimeSpan Time,
        string ArticleName
    );

}
