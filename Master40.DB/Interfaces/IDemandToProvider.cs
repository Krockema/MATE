using Master40.DB.Data.WrappersForPrimitives;
using Master40.DB.DataModel;

namespace Master40.DB.Interfaces
{
    public interface IDemandToProvider
    {
        int Id { get; set; }
        int DemandId { get; set; }
        int ProviderId { get; set; }
        
        decimal Quantity { get; set; }

        Id GetProviderId();

        Id GetDemandId();
    }
}
