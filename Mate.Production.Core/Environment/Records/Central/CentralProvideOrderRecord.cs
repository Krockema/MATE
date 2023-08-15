using System;

namespace Mate.Production.Core.Environment.Records.Central
{
    public record CentralProvideOrderRecord
    (
        string ProductionOrderId,
        string MaterialId,
        string MaterialName,
        string SalesOrderId,
        DateTime MaterialFinishedAt
    );
}
