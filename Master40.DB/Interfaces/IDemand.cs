using Master40.DB.Data.WrappersForPrimitives;
using Master40.DB.DataModel;

namespace Master40.DB.Interfaces
{
    public interface IDemand
    {
        int Id { get; set; }
        int? DemandId { get; set; }
        T_Demand Demand { get; set; }

        Quantity GetQuantity();
    }
}