using System.Collections.Generic;
using Master40.DB.Data.WrappersForPrimitives;

namespace Zpp.Mrp2.impl.Mrp1.impl.LotSize
{
    public interface ILotSize
    {
        List<Quantity> GetLotSizes();
    }
}