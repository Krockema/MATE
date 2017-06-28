using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Master40.DB.Data.Helper
{
    public static class LinqExtensions
    {
        public static void CopyPropertiesTo<T>(this T source, T dest)
        {
            var plist = from prop in typeof(T).GetProperties() where prop.CanRead && prop.CanWrite select prop;

            foreach (PropertyInfo prop in plist)
            {
                if (prop.Name != "Id")
                    prop.SetValue(dest, prop.GetValue(source, null), null);
            }
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
