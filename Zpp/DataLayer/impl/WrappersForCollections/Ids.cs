using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Master40.DB.Data.WrappersForPrimitives;
using Zpp.Util.StackSet;

namespace Zpp.DataLayer.impl.WrappersForCollections
{
    /**
     * wraps ids, emulates a Set, since we cannot use StackSet here
     */
    public class Ids: IEnumerable<Id>
    {
        private readonly Dictionary<Id, Id> _ids = new Dictionary<Id, Id>();

        public void Add(Id id)
        {
            _ids.Add(id, id);
        }
        
        public void Remove(Id id)
        {
            _ids.Remove(id);
        }

        public bool Contains(Id id)
        {
            return _ids.ContainsKey(id);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _ids.Keys.GetEnumerator();
        }

        public IEnumerator<Id> GetEnumerator()
        {
            return _ids.Keys.GetEnumerator();
        }
    }
}