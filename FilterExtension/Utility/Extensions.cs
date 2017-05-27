using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace FilterExtensions.Utility
{
    public struct Span<T>
    {
        private IList<T> spanningCollection;
        private readonly int offset;
        public int Length { get; }

        public Span(IList<T> collection, int offset, int length)
        {
            Debug.Assert(offset + length <= collection.Count, "Span can't cover a larger range than the collection covers");
            spanningCollection = collection;
            this.offset = offset;
            Length = length;
        }

        public T this[int index]
        {
            get
            {
                Debug.Assert(index < Length, "index out of range");
                return spanningCollection[index + offset];
            }
            set 
            {
                Debug.Assert(index < Length, "index out of range");
                spanningCollection[index + offset] = value;
            }
        }
    }

    public static class Extensions
    {

        /// <summary>
        /// adds the (key, value) set to the Dictionary if the key is unique
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="U"></typeparam>
        /// <param name="addTo"></param>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static bool TryAdd<T, U>(this IDictionary<T, U> addTo, T key, U value)
        {
            if (addTo.ContainsKey(key))
            {
                return false;
            }
            addTo.Add(key, value);
            return true;
        }

        /// <summary>
        /// TryGetValue for a list with predicate
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static bool TryGetValue<T>(this IList<T> list, Func<T, bool> match, out T value) where T : class
        {
            foreach (T t in list)
            {
                if (match(t))
                {
                    value = t;
                    return true;
                }
            }
            value = null;
            return false;
        }

        public static bool PMListContains(this PartModuleList list, string moduleName)
        {
            for (int i = 0; i < list.Count; ++i)
            {
                if (list[i] != null && list[i].GetType().Name == moduleName)
                {
                    return true;
                }
            }
            return false;
        }
    }
}