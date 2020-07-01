using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Microsoft.EntityFrameworkCore;

namespace Master40.DB.Data.Helper
{
    public class TypeResolver
    {
        public IQueryable<dynamic> GetDbSetByType(string fullname)
        {
            Type targetType = Type.GetType(typeName: fullname);

            var model = GetType()
                .GetRuntimeProperties()
                .FirstOrDefault(predicate: o => o.PropertyType.GetGenericTypeDefinition() == typeof(DbSet<>) &&
                    o.PropertyType.GenericTypeArguments.Contains(value: targetType));

            return (IQueryable<dynamic>) model?.GetValue(obj: this);
        }
    }
}
