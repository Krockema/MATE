using Master40.DB.Data.Context;
using Master40.DB;
using Master40.DB.Data.Repository;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Linq.Expressions;

namespace Master40.DB.Data.Repository
{
    public class Repository<T> : IRepository<T> where T : BaseEntity
    {
        private readonly MasterDBContext context;
        private DbSet<T> entities;
        string errorMessage = string.Empty;

        public Repository(MasterDBContext context)
        {
            this.context = context;
            entities = context.Set<T>();
        }
        public IQueryable<T> GetAll()
        {
            return entities;
        }
        public IQueryable<T> Find(Expression<Func<T, Boolean>> predicate)
        {
            return entities.Where(predicate);
        }


        public T Get(long id)
        {
            return entities.SingleOrDefault(s => s.Id == id);
        }
        public void Insert(T entity)
        {
            if (entity == null)
            {
                throw new ArgumentNullException("entity");
            }
            entities.Add(entity);
            context.SaveChanges();
        }

        public void Update(T entity)
        {
            if (entity == null)
            {
                throw new ArgumentNullException("entity");
            }
            context.SaveChanges();
        }

        public void Delete(T entity)
        {
            if (entity == null)
            {
                throw new ArgumentNullException("entity");
            }
            entities.Remove(entity);
            context.SaveChanges();
        }

        public void Dispose()
        {
            context.Dispose();
        }

    }
}
