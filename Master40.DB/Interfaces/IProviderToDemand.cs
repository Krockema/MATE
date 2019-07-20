using Master40.DB.Data.WrappersForPrimitives;
using Master40.DB.DataModel;

namespace Master40.DB.Interfaces
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