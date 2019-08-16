using Master40.DB.DataModel;

namespace Master40.DB.Interfaces
{
    public interface IDemand
    {
        int Id { get; set; }
        int? DemandID { get; set; }
        T_Demand Demand { get; set; }
    }
}