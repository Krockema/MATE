using System.Collections.Generic;
using Master40.DB.Data.WrappersForPrimitives;

namespace Zpp.LotSize
{
    public interface ILotSize
    {
        List<Quantity> GetCalculatedQuantity();
    }
}