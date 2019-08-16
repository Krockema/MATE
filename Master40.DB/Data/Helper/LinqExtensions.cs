using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.EntityFrameworkCore;


namespace Master40.DB.Data.Helper
{
    public static class LinqExtensions
    {
        public static long GetEpochMilliseconds(this DateTime date)
        {
            double ticks = 47 * 60 * 60 * 1000;
            var startdate = DateTime.Now.AddMilliseconds(value: -ticks);
            TimeSpan t = startdate - new DateTime(year: 1970, month: 1, day: 1);
            return (long)t.TotalMilliseconds;
        }

        public static DateTime GetDateFromMilliseconds(this long x)
        {
            // return new DateTime(1970, 1, 1).Add(TimeSpan.FromMilliseconds(x));
            return (new DateTime(year: 1970, month: 1, day: 1)).AddMilliseconds(value: x);
        }

        public static void WriteCSV<T>(this IEnumerable<T> items, string path)
        {
            Type itemType = typeof(T);
            var props = itemType.GetProperties(bindingAttr: BindingFlags.Public | BindingFlags.Instance)
                                .OrderBy(keySelector: p => p.Name);

            using (var writer = new StreamWriter(path: path))
            {
                writer.WriteLine(value: string.Join(separator: ", ", values: props.Select(selector: p => p.Name)));

                foreach (var item in items)
                {
                    writer.WriteLine(value: string.Join(separator: ", ", values: props.Select(selector: p => p.GetValue(obj: item, index: null))));
                }
            }
        }

        public static void CopyPropertiesTo<T>(this T source, T dest)
        {
            var plist = from prop in typeof(T).GetProperties() where prop.CanRead && prop.CanWrite select prop;

            foreach (PropertyInfo prop in plist)
            {
                if (prop.Name != "Id")
                    prop.SetValue(obj: dest, value: prop.GetValue(obj: source, index: null), index: null);
            }
        }

        public static dynamic CopyProperties<T>(this T source)
        {
            // create A specific instance of the sourcetype
            var dest = Activator.CreateInstance(type: source.GetType());
            // get property list from Sourcetype
            var plist = from prop in dest.GetType().GetProperties() where prop.CanRead && prop.CanWrite select prop;
            // copy item without navigation properties
            foreach (PropertyInfo prop in plist)
            {   
                if (!prop.PropertyType.Name.Contains(value: "ICollection") && !prop.PropertyType.FullName.Contains(value: "Master40.DB.Models"))
                    prop.SetValue(obj: dest, value: prop.GetValue(obj: source, index: null), index: null);
            }
            return dest;
        }

        public static void CopyDbPropertiesTo<T>(this T source, T dest)
        {
            var plist = from prop in typeof(T).GetProperties() where prop.CanRead && prop.CanWrite select prop;

            foreach (PropertyInfo prop in plist)
            {
                if (prop.Name != "Id" && !prop.PropertyType.Name.Contains(value: "ICollection") && !prop.PropertyType.FullName.Contains(value: "Master40.DB.Models"))
                    prop.SetValue(obj: dest, value: prop.GetValue(obj: source, index: null), index: null);
            }
        }

        public static dynamic CopyDbPropertiesWithoutId<T>(this T source)
        {
            var plist = from prop in typeof(T).GetProperties() where prop.CanRead && prop.CanWrite select prop;
            var dest = Activator.CreateInstance(type: source.GetType());
            foreach (PropertyInfo prop in plist)
            {
                if (prop.Name != "Id" && !prop.PropertyType.Name.Contains(value: "ICollection") && !prop.PropertyType.FullName.Contains(value: "Master40.DB.Models"))
                    prop.SetValue(obj: dest, value: prop.GetValue(obj: source, index: null), index: null);
            }

            return dest;
        }
        public static List<T> ResetId<T>(this List<T> entity) where T : IBaseEntity
        {
            return entity.Select(selector: x => { x.Id = 0; return x; }).ToList();
        }


        public static List<PropertyInfo> GetDbSetProperties(this DbContext context)
        {
            var properties = context.GetType().GetProperties();
            return (from property in properties let setType = property.PropertyType let isDbSet = (typeof(DbSet<>)
                    .IsAssignableFrom(c: setType.GetGenericTypeDefinition())) where isDbSet select property).ToList();

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
            var stack = new Stack<T>(collection: new T[] {item});

            while (stack.Any())
            {
                var next = stack.Pop();
                if (next != null)
                {
                    yield return next;
                    stack.Push(item: childSelector(arg: next));
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
            var stack = new Stack<T>(collection: new T[] {item});

            while (stack.Any())
            {
                var next = stack.Pop();
                //if(next != null)
                //{
                yield return next;
                foreach (var child in childSelector(arg: next))
                {
                    stack.Push(item: child);
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
            var stack = new Stack<T>(collection: items);
            while (stack.Any())
            {
                var next = stack.Pop();
                yield return next;
                foreach (var child in childSelector(arg: next))
                    stack.Push(item: child);
            }
        }
    }
}
