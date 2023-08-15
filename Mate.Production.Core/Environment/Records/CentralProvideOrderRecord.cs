
using System;

public record CentralProvideOrderRecord(
    string ProductionOrderId,
    string MaterialId,
    string MaterialName,
    string SalesOrderId,
    DateTime MaterialFinishedAt
);
