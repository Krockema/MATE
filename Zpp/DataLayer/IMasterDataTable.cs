using System.Collections.Generic;
using Master40.DB;
using Master40.DB.Data.WrappersForPrimitives;

namespace Zpp.DataLayer
{
    public interface IMasterDataTable<T> : IEnumerable<T> where T : BaseEntity
    {
        T GetById(Id id);

        List<T> GetAll();

        void SetAll(List<T> entityList);
    }
}