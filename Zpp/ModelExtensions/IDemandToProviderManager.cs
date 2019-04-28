using System.Collections.Generic;

namespace Zpp.ModelExtensions
{
    public interface IDemandToProviderManager
    {
        List<Demand> orderByUrgency(List<Demand> demands);
    }
}