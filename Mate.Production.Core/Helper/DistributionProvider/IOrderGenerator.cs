using Mate.DataCore.DataModel;
using Mate.Production.Core.Environment.Options;

namespace Mate.Production.Core.Helper.DistributionProvider
{
    public interface IOrderGenerator
    {
        T_CustomerOrder GetNewRandomOrder(long time);
        OrderArrivalRate GetOrderArrivalRate();
    }
}
