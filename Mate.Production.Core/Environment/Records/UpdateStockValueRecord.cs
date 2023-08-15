namespace Mate.Production.Core.Environment.Records
{
    public record UpdateStockValueRecord(
    string StockName,
    double NewValue,
    string ArticleType);
}