using Zpp.Mrp2.impl.Mrp1.impl.LotSize.Impl;

namespace Zpp.Utils
{
    public class CentralPlanningConfiguration : ICentralPlanningConfiguration
    {
    public int CustomerOrderPartQuantity { get; set; }
    public int LotSize { get; set; }

    public LotSizeType LotSizeType { get; set; }
    public string Name { get; set; }

    // classname as full AssemblyQualifiedName see MSDoc AssemblyQualifiedName
    public string DbSetInitializer { get; set; }

    public int SimulationMaximumDuration { get; set; } = 20160;

    public int SimulationInterval { get; set; } = 1440;
    }
}