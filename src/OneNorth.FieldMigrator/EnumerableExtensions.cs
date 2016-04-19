using System;
using System.Collections.Generic;

namespace OneNorth.FieldMigrator
{
    public static class EnumerableExtensions
    {
        public static IEnumerable<T> Parents<T>(this T source, Func<T, T> parentSelector)
        {
            var parent = parentSelector(source);

            while (parent != null)
            {
                yield return parent;
                parent = parentSelector(parent);
            }
        }

        public static IEnumerable<T> Flatten<T>(this T source, Func<T, IEnumerable<T>> childrenSelector)
        {
            var stack = new Stack<T>();
            stack.Push(source);

            while (stack.Count > 0)
            {
                var current = stack.Pop();
                yield return current;

                var children = childrenSelector(current);
                if (children == null) continue;

                foreach (var child in children)
                    stack.Push(child);
            }
        }
    }
}