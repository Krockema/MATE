using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Master40.DB;
using Master40.DB.Data.WrappersForPrimitives;
using Microsoft.EntityFrameworkCore;
using Zpp.Util;

namespace Zpp.DataLayer.impl
{
    public class MasterDataTable<T> : IMasterDataTable<T> where T : BaseEntity
    {
        private readonly Dictionary<Id, T> _entitesAsDictionary;
        private List<T> _entities;

        public MasterDataTable(DbSet<T> entitySet)
        {
            _entities = entitySet.ToList();
            _entitesAsDictionary = entityListToDictionary(_entities);
        }
        
        public MasterDataTable(List<T> entitySetAsList)
        {
            _entities = entitySetAsList;
            _entitesAsDictionary = entityListToDictionary(_entities);
        }
        
        private Dictionary<Id, T> entityListToDictionary<T>(List<T> entityList) where T : BaseEntity
        {
            Dictionary<Id, T> dictionary = new Dictionary<Id, T>();
            foreach (var entity in entityList)
            {
                dictionary.Add(entity.GetId(), entity);
            }

            return dictionary;
        }

        public T GetById(Id id)
        {
            if (!_entitesAsDictionary.ContainsKey(id))
            {
                throw new MrpRunException($"Given id ({id}) is not present in this masterDataTable.");
            }
            return _entitesAsDictionary[id];
        }

        public List<T> GetAll()
        {
            return _entities;
        }

        public void SetAll(List<T> entityList)
        {
            _entities = entityList;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _entities.GetEnumerator();
        }

        public IEnumerator<T> GetEnumerator()
        {
            return _entities.GetEnumerator();
        }
    }
}