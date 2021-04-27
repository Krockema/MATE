using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.EntityFrameworkCore;

namespace Mate.DataCore.Data.Helper
{
    public static class LinqExtensions
    {
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
    }
}
