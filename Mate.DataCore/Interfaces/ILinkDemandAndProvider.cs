using Mate.DataCore.Data.WrappersForPrimitives;

namespace Mate.DataCore.Interfaces
{
    public interface ILinkDemandAndProvider: IId
    {
        int Id { get; set; }
        int DemandId { get; set; }
        int ProviderId { get; set; }
        
        decimal? Quantity { get; set; }
        
        Id GetProviderId();

        Id GetDemandId();

        Quantity GetQuantity();
    }
}