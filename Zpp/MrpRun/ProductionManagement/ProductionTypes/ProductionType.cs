namespace Zpp.MrpRun.ProductionManagement.ProductionTypes
{
    public enum ProductionType
    {
        // https://wirtschaftslexikon.gabler.de/definition/fliessproduktion-32644
        AssemblyLine,
        // https://wirtschaftslexikon.gabler.de/definition/werkstattproduktion-46964
        WorkshopProductionClassic,
        // As WorkshopProductionClassic, but for every articleBom one ProductionOrderPart
        WorkshopProduction
    }
}