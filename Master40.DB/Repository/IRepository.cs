using System;
using System.Collections.Generic;
using System.Text;

namespace Master40.DB.Repository
{ 
    public interface IRepository<TEntity>
    {
        TEntity Get(int id);
        IEnumerable<TEntity> ToList();
        IEnumerable<TEntity> Where(Func<TEntity, bool> predicate);
        TEntity SingleOrDefault(Func<TEntity, bool> predicate);
        void Add(object parrent, TEntity entity, string fKey);
        void AddRange(IEnumerable<TEntity> entities);
        void Remove(TEntity entity, string fKey);
        void RemoveRange(IEnumerable<TEntity> entities, string fKey);
        int Count(Func<TEntity, bool> predicate);
        int Count();

    }
}
