using Master40.DB.Data.WrappersForPrimitives;
using Master40.DB.Interfaces;

namespace Master40.DB.DataModel
{
    /// <summary>
    /// derived Class for Damand to DemandProvider
    /// To Access a specific Demand use:
    /// var purchaseDemands = context.Demands.OfType<DemandPurchase>().ToList();
    /// </summary>
    public class T_DemandToProvider : BaseEntity, IDemandToProvider
    {
        public int DemandId { get; set; }
        public int ProviderId { get; set; }
        
        public decimal Quantity { get; set; }

        public override string ToString()
        {
            return $"demand: {DemandId}, provider: {ProviderId}";
        }

        public Id GetProviderId()
        {
            return new Id(ProviderId);
        }

        public Id GetDemandId()
        {
            return new Id(DemandId);
        }
    }
}
