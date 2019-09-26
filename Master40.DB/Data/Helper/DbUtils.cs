using Master40.DB.Data.Context;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Master40.DB.Data.Helper
{
    public static class DbUtils
    {

        public static void InsertOrUpdateRange<TEntity>(IEnumerable<TEntity> entities,
            DbSet<TEntity> dbSet, MasterDBContext masterDBContext) where TEntity : BaseEntity
        {
            if (entities.Any() == false)
            {
                throw new SystemException("Collection to persist is empty.");
            }

            // dbSet.AddRange(entities);
            foreach (var entity in entities)
            {
                // e.g. if it is a PrBom which is toPurchase
                if (entity != null)
                {
                    InsertOrUpdate(entity, dbSet, masterDBContext);
                }
            }
        }

        public static void InsertOrUpdate<TEntity>(TEntity entity, DbSet<TEntity> dbSet, MasterDBContext masterDBContext)
            where TEntity : BaseEntity
        {
            TEntity foundEntity = dbSet.Find(entity.Id);
            if (foundEntity == null
                ) // TODO: performance issue: a select before every insert is a no go
                  // it's not in DB yet
            {
                masterDBContext.Entry(entity).State = EntityState.Added;
                dbSet.Add(entity);
            }
            else
            // it's already in DB
            {
                CopyDbPropertiesTo(entity, foundEntity);
                masterDBContext.Entry(foundEntity).State = EntityState.Modified;
                dbSet.Update(foundEntity);
            }
        }
        public static void CopyDbPropertiesTo<T>(T source, T dest)
        {
            var plist = from prop in typeof(T).GetProperties() where prop.CanRead && prop.CanWrite select prop;

            foreach (PropertyInfo prop in plist)
            {
                // ToDo: are there more primitives?
                if (prop.PropertyType.IsPrimitive
                    || prop.PropertyType == typeof(string)
                    || prop.PropertyType == typeof(DateTime)
                    || prop.PropertyType == typeof(Guid)
                    || prop.PropertyType == typeof(int?)
                    || prop.PropertyType.BaseType == typeof(Enum))
                    prop.SetValue(dest, prop.GetValue(source, null), null);
            }
        }

    }
}
