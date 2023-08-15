namespace Mate.Production.Core.Environment.Records.Central
{
    public record ResourceHubInformationRecord
    (
        object ResourceList,
        string DbConnectionString,
        string MasterDbConnectionString,
        string PathToGANTTPLANOptRunner,
        object WorkTimeGenerator
    );
}

