using Master40.DB.DataModel;

namespace Master40.DB.Interfaces
{
    public interface IDemandToProvider
    {
        int Id { get; set; }
        int DemandId { get; set; }
        T_Demand Demand { get; set; }
        int ProviderId { get; set; }
        T_Provider Provider { get; set; }
    }
}
