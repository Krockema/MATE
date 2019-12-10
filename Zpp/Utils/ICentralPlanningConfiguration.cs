using System;
using System.Collections.Generic;
using System.Text;
using Zpp.Mrp2.impl.Mrp1.impl.LotSize.Impl;

namespace Zpp.Utils
{
    public interface ICentralPlanningConfiguration
    {
        int CustomerOrderPartQuantity { get; set; }
        int LotSize { get; set; }

        LotSizeType LotSizeType { get; set; }
        string Name { get; set; }

        // classname as full AssemblyQualifiedName see MSDoc AssemblyQualifiedName
        string DbSetInitializer { get; set; }

        int SimulationMaximumDuration { get; set; }

        int SimulationInterval { get; set; }
    }
}
