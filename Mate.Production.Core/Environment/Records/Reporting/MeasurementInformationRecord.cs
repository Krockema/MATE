namespace Mate.Production.Core.Environment.Records.Reporting
{
    public record MeasurementInformationRecord
    (
        IJob Job,
        string Resource,
        double Quantile,
        string Tool,
        int CapabilityProviderId,
        string Bucket
    );
}

