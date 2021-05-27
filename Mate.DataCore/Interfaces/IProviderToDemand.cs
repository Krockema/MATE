using Mate.DataCore.Data.WrappersForPrimitives;

namespace Mate.DataCore.Interfaces
{
    public interface IProviderToDemand
    {
        int Id { get; set; }
        int ProviderId { get; set; }
        int DemandId { get; set; }
        
        Id GetProviderId();

        Id GetDemandId();
    }
}