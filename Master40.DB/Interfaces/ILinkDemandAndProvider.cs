using Master40.DB.Data.WrappersForPrimitives;

namespace Master40.DB.Interfaces
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