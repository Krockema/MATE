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
            Type targetType = Type.GetType(fullname);

            var model = GetType()
                .GetRuntimeProperties()
                .FirstOrDefault(o => o.PropertyType.GetGenericTypeDefinition() == typeof(DbSet<>) &&
                    o.PropertyType.GenericTypeArguments.Contains(targetType));

            return (IQueryable<dynamic>) model?.GetValue(this);
        }
    }
}
