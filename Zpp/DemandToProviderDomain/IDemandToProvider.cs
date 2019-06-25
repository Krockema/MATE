using Zpp.DemandDomain;
using Zpp.ProviderDomain;

namespace Zpp.DemandToProviderDomain
{
    public interface IDemandToProviderLogic
    {
        bool IsSatisfied(Demand demand);
    }
}