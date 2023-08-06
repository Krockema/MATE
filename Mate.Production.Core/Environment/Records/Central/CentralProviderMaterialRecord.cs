namespace Mate.Production.Core.Environment.Records.Central
{
    public record CentralProvideMaterialRecord
    (
        string ProductionOrderId,
        string OperationId,
        int ActivityId,
        string MaterialId
    );
}
