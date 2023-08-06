namespace Mate.Production.Core.Environment.Records.Central
{
    public record CentralPurchaseRecord
    (
        int MaterialId,
        string MaterialName,
        double Quantity
    );
}
