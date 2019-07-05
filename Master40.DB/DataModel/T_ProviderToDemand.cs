using Master40.DB.Interfaces;

namespace Master40.DB.DataModel
{
    public class T_ProviderToDemand : BaseEntity, IProviderToDemand
    {
        public int ProviderId { get; set; }
        public T_Provider Provider { get; set; }
        public int DemandId { get; set; }
        public T_Demand Demand { get; set; }
        
        public override string ToString()
        {
            return $"provider: {ProviderId}, demand: {DemandId}";
        }
    }
}