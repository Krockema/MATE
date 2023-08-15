using System;


public record ThroughPutTimeRecord(
    Guid ArticleKey,
    string ArticleName,
    DateTime Start ,
    DateTime End);
