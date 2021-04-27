using Mate.DataCore.Data.WrappersForPrimitives;

namespace Mate.DataCore.Interfaces
{
    public interface IDemand: IBaseEntityCodeGeneratedId
    {
        Quantity GetQuantity();
    }
}