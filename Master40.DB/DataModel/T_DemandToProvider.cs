namespace Master40.DB.DataModel
{
    public interface IDemandToProvider
    {
        int Id { get; set; }
        int DemandId { get; set; }
        T_Demand Demand { get; set; }
        int ProviderId { get; set; }
        T_Provider Provider { get; set; }
    }

    /// <summary>
    /// derived Class for Damand to DemandProvider
    /// To Access a specific Demand use:
    /// var purchaseDemands = context.Demands.OfType<DemandPurchase>().ToList();
    /// </summary>
    public class T_DemandToProvider : BaseEntity, IDemandToProvider
    {
        public int DemandId { get; set; }
        public T_Demand Demand { get; set; }
        public int ProviderId { get; set; }
        public T_Provider Provider { get; set; }
    }
}
