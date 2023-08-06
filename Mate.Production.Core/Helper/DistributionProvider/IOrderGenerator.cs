using Mate.DataCore.DataModel;
using Mate.Production.Core.Environment.Options;
using System;

namespace Mate.Production.Core.Helper.DistributionProvider
{
    public interface IOrderGenerator
    {
        T_CustomerOrder GetNewRandomOrder(DateTime time);
        OrderArrivalRate GetOrderArrivalRate();
    }
}
