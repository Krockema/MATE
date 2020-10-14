module FCentralProvideOrders

type public FCentralProvideOrder = {
    ProductionOrderId: string
    MaterialId : string
    MaterialName : string
    SalesOrderId : string
    MaterialFinishedAt : int64
}