using Mate.DataCore.Data.WrappersForPrimitives;
using Mate.DataCore.Interfaces;

namespace Mate.DataCore.DataModel
{
    public class T_ProviderToDemand : BaseEntity, ILinkDemandAndProvider
    {
        public int ProviderId { get; set; }
        public int DemandId { get; set; }
        
        public decimal? Quantity { get; set; }

        public T_ProviderToDemand()
        {
        }

        public T_ProviderToDemand(Id providerId, Id demandId, Quantity quantity)
        {
            ProviderId = providerId.GetValue();
            DemandId = demandId.GetValue();
            if (quantity != null)
            {
                Quantity = quantity.GetValue();   
            }
        }

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

        public Quantity GetQuantity()
        {
            return new Quantity((decimal) Quantity);
        }
        
        public override bool Equals(object obj)
        {
            T_ProviderToDemand other = (T_ProviderToDemand) obj;
            return DemandId.Equals(other.DemandId) && ProviderId.Equals(other.ProviderId);
        }

        public override int GetHashCode()
        {
            return DemandId.GetHashCode() + ProviderId.GetHashCode();
        }
    }
}