using Master40.DB.Data.Context;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;


namespace Master40.DB.Repository
{
    public class Repository<T> : IRepository<T> where T : BaseEntity
    {
        protected readonly MasterDBContext Context;
        HashSet<T> _localStore;
        private UnitOfWork _unitOfWork;
        private int index = 0;
        public Repository(MasterDBContext context, UnitOfWork unitOfWork)
        {
            Context = context;
            if (context.InMemory)
            {
                _localStore = new HashSet<T>();
                _unitOfWork = unitOfWork;
            }
        }

        public T Get(int id)
        {
            if (Context.InMemory)
                return _localStore.Single(x => x.Id == id);
            else
                return Context.Set<T>().Find(id);
        }

        public IEnumerable<T> ToList()
        {
            if (Context.InMemory)
                return _localStore.ToList();
            else
                return Context.Set<T>().ToList();
        }
        public IEnumerable<T> Where(Func<T, bool> predicate)
        {
            if (Context.InMemory)
                return _localStore.Where(predicate);
            else
                return Context.Set<T>().Where(predicate);
        }

        public T SingleOrDefault(Func<T, bool> predicate)
        {
            if (Context.InMemory)
                return _localStore.SingleOrDefault(predicate);
            else
                return Context.Set<T>().SingleOrDefault(predicate);
        }

        public void Add(IBaseEntity parent, T entity, string fKey)
        {
            if (Context.InMemory)
            {
                AddLocal(parent, entity, fKey);
            }
            else
            {
                Context.Set<T>().Add(entity);
            }

        }
        private void AddLocal(IBaseEntity parent, T entity, string fKey)
        {
            entity.Id = index++;
            entity.GetType().GetProperty(fKey + "Id").SetValue(entity, parent.Id);
            entity.GetType().GetProperty(fKey).SetValue(entity, parent);
            _localStore.Add(entity);
        }

        public void AddRange(IBaseEntity parent ,IEnumerable<T> entities, string fKey)
        {
            if (Context.InMemory)
                foreach (var item in entities)
                    AddLocal(parent, item, fKey);
            else
                Context.Set<T>().AddRange(entities);
        }

        public void Remove(T entity, string fKey)
        {
            if (Context.InMemory)
            {
                _localStore.Remove(entity);
                RemoveBinding(entity, fKey);
            }
            else
                Context.Set<T>().Remove(entity);
        }

        public void RemoveRange(IEnumerable<T> entities, string fKey)
        {
            if (Context.InMemory)
                foreach (var item in entities)
                    Remove(item, fKey);
            else
                Context.Set<T>().RemoveRange(entities);
        }
        private void RemoveBinding(T entity, string fKey)
        {
            entity.GetType().GetProperty(fKey + "Id").SetValue(entity, null);
            entity.GetType().GetProperty(fKey).SetValue(entity, null);
        }


        public int Count(Func<T, bool> predicate)
        {
            if (Context.InMemory)
                return _localStore.Count(predicate);
            else
                return Context.Set<T>().Count(predicate);

        }

        public int Count()
        {
            if (Context.InMemory)
                return _localStore.Count();
            else
                return Context.Set<T>().Count();

        }

        internal IQueryable<T> Include(string include)
        {
            if (Context.InMemory)
                return _localStore.AsQueryable();
            else
                return Context.Set<T>().Include(include);
        }

        public void Add(object parrent, T entity, string fKey)
        {
            throw new NotImplementedException();
        }

        public void AddRange(IEnumerable<T> entities)
        {
            throw new NotImplementedException();
        }
    }
}
