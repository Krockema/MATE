using System;

namespace Mate.Production.Core.Environment.Records.Reporting
{
    public record ThroughPutTImeRecord
    (
        Guid ArticleKey,
        string ArticleName,
        DateTime Start,
        DateTime End
    );
}

