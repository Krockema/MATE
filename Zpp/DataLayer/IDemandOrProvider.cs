using Master40.DB.Data.WrappersForPrimitives;
using Zpp.Util.Graph;

namespace Zpp.DataLayer
{
    public interface IDemandOrProvider: IScheduleNode
    {
        Quantity GetQuantity();
        
    }
}