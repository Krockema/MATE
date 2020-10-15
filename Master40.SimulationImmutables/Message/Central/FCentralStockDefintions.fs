module FCentralStockDefinitions
    type public FCentralStockDefinition = {
        StockId : int
        MaterialName: string
        InitialQuantity : double
        Unit : string
        Price: double
        MaterialType : string
        DeliveryPeriod : int64
    }