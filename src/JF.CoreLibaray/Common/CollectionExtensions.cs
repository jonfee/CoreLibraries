using System;
using System.Collections.Generic;
using System.Text;

namespace JF.Common
{
    /// <summary>
    /// 集合类扩展
    /// </summary>
   public static class CollectionExtensions
    {
        public static void ForEach<T>(this IEnumerable<T> items, Action<T> action)
        {
            if (action == null) return;

            foreach (T item in items)
            {
                action(item);
            }
        }
    }
}
