using Master40.DB.Data.WrappersForPrimitives;
using Master40.DB.Interfaces;

namespace Master40.DB.DataModel
{
    public class T_ProviderToDemand : BaseEntity, IProviderToDemand
    {
        public int ProviderId { get; set; }
        public int DemandId { get; set; }

        public override string ToString()
        {
            return $"provider: {ProviderId}, demand: {DemandId}";
        }
        
        public Id GetProviderId()
        {
            return new Id(ProviderId);
        }

        public Id GetDemandId()
        {
            return new Id(DemandId);
        }

        public T_DemandToProvider ToDemandToProvider()
        {
            T_DemandToProvider demandToProvider = new T_DemandToProvider();
            demandToProvider.DemandId = DemandId;
            demandToProvider.ProviderId = ProviderId;
            return demandToProvider;
        }
    }
}