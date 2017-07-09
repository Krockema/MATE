using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Remotion.Linq.Parsing.Structure.IntermediateModel;

namespace Master40.DB.Data.Helper
{
    public static class LinqExtensions
    {
        public static long GetEpochMilliseconds(this DateTime date)
        {
            TimeSpan t = DateTime.UtcNow - new DateTime(1970, 1, 1);
            return (long)t.TotalMilliseconds;
        }

        public static DateTime GetDateFromMilliseconds(this long x)
        {
            // return new DateTime(1970, 1, 1).Add(TimeSpan.FromMilliseconds(x));
            return (new DateTime(1970, 1, 1)).AddMilliseconds(x);
        }


        public static void CopyPropertiesTo<T>(this T source, T dest)
        {
            var plist = from prop in typeof(T).GetProperties() where prop.CanRead && prop.CanWrite select prop;

            foreach (PropertyInfo prop in plist)
            {
                if (prop.Name != "Id")
                    prop.SetValue(dest, prop.GetValue(source, null), null);
            }
        }

        public static dynamic CopyProperties<T>(this T source)
        {
            // create A specific instance of the sourcetype
            var dest = Activator.CreateInstance(source.GetType());
            // get property list from Sourcetype
            var plist = from prop in dest.GetType().GetProperties() where prop.CanRead && prop.CanWrite select prop;
            // copy item without navigation properties
            foreach (PropertyInfo prop in plist)
            {   
                if (!prop.PropertyType.Name.Contains("ICollection") && !prop.PropertyType.FullName.Contains("Master40.DB.Models"))
                    prop.SetValue(dest, prop.GetValue(source, null), null);
            }
            return dest;
        }

        public static void CopyDbPropertiesTo<T>(this T source, T dest)
        {
            var plist = from prop in typeof(T).GetProperties() where prop.CanRead && prop.CanWrite select prop;

            foreach (PropertyInfo prop in plist)
            {
                if (!prop.PropertyType.Name.Contains("ICollection") && !prop.PropertyType.FullName.Contains("Master40.DB.Models"))
                    prop.SetValue(dest, prop.GetValue(source, null), null);
            }
        }


        public static List<PropertyInfo> GetDbSetProperties(this DbContext context)
        {
            var properties = context.GetType().GetProperties();
            return (from property in properties let setType = property.PropertyType let isDbSet = (typeof(DbSet<>)
                    .IsAssignableFrom(setType.GetGenericTypeDefinition())) where isDbSet select property).ToList();

        }

        /// <summary>
        /// Return item and all children recursively.
        /// </summary>
        /// <typeparam name="T">Type of item.</typeparam>
        /// <param name="item">The item to be traversed.</param>
        /// <param name="childSelector">Child property selector.</param>
        /// <returns></returns>
        public static IEnumerable<T> Traverse<T>(this T item, Func<T, T> childSelector)
        {
            var stack = new Stack<T>(new T[] {item});

            while (stack.Any())
            {
                var next = stack.Pop();
                if (next != null)
                {
                    yield return next;
                    stack.Push(childSelector(next));
                }
            }
        }

        /// <summary>
        /// Return item and all children recursively.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="item"></param>
        /// <param name="childSelector"></param>
        /// <returns></returns>
        public static IEnumerable<T> Traverse<T>(this T item, Func<T, IEnumerable<T>> childSelector)
        {
            var stack = new Stack<T>(new T[] {item});

            while (stack.Any())
            {
                var next = stack.Pop();
                //if(next != null)
                //{
                yield return next;
                foreach (var child in childSelector(next))
                {
                    stack.Push(child);
                }
                //}
            }
        }

        /// <summary>
        /// Return item and all children recursively.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="items"></param>
        /// <param name="childSelector"></param>
        /// <returns></returns>
        public static IEnumerable<T> Traverse<T>(this IEnumerable<T> items,
            Func<T, IEnumerable<T>> childSelector)
        {
            var stack = new Stack<T>(items);
            while (stack.Any())
            {
                var next = stack.Pop();
                yield return next;
                foreach (var child in childSelector(next))
                    stack.Push(child);
            }
        }
    }
}
