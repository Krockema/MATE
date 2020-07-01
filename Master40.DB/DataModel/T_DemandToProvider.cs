using Master40.DB.Data.WrappersForPrimitives;
using Master40.DB.Interfaces;

namespace Master40.DB.DataModel
{
    /// <summary>
    /// derived Class for Damand to DemandProvider
    /// To Access a specific Demand use:
    /// var purchaseDemands = context.Demands.OfType<DemandPurchase>().ToList();
    /// </summary>
    public class T_DemandToProvider : BaseEntity, ILinkDemandAndProvider
    {
        public int DemandId { get; set; }
        public int ProviderId { get; set; }

        public decimal? Quantity { get; set; } = null;

        public T_DemandToProvider()
        {
        }

        public T_DemandToProvider(Id demandId, Id providerId, Quantity quantity)
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
            return $"demand: {DemandId}, provider: {ProviderId}, quantity: {Quantity}";
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
            T_DemandToProvider other = (T_DemandToProvider) obj;
            return DemandId.Equals(other.DemandId) && ProviderId.Equals(other.ProviderId);
        }

        public override int GetHashCode()
        {
            return DemandId.GetHashCode() + ProviderId.GetHashCode();
        }
    }
}
