using Master40.DB.DataModel;
using Master40.SimulationCore.Environment.Options;

namespace Master40.SimulationCore.Helper.DistributionProvider
{
    public interface IOrderGenerator
    {
        T_CustomerOrder GetNewRandomOrder(long time);

        OrderArrivalRate GetOrderArrivalRate();
    }
}
