module FCentralStockDefinitions
    type public FCentralStockDefinition = {
        StockId : int
        StockName: string
        InitialQuantity : double
        Unit : string
        MaterialType : string
        DeliveryPeriod : double
    }