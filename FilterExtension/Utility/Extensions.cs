using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace FilterExtensions.Utility
{
    public static class Extensions
    {
        /// <summary>
        /// adds the <key, value> set to the Dictionary if the key is unique
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
                return false;
            addTo.Add(key, value);
            return true;
        }

        /// <summary>
        /// get the first element in a collection that is of Type T, or inherits from type T
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list"></param>
        /// <returns></returns>
        public static T FirstOfTypeOrDefault<T>(this IList<T> list) where T : class
        {
            for (int i = 0; i < list.Count; i++)
            {
                T module = list[i] as T;
                if (module != null)
                    return module;
            }
            return default(T);
        }

        /// <summary>
        /// get the first element in a collection that is of Type T, or inherits from type T
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list"></param>
        /// <returns></returns>
        public static T FirstOfTypeOrDefault<T>(this PartModuleList list) where T : class
        {
            for (int i = 0; i < list.Count; i++)
            {
                T module = list[i] as T;
                if (module != null)
                    return module;
            }
            return default(T);
        }

        /// <summary>
        /// get the first element in a collection that is of Type T, or inherits from type T
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="set"></param>
        /// <returns></returns>
        public static T FirstOfTypeOrDefault<T>(this System.Collections.IEnumerable set) where T : class
        {
            foreach (T item in set)
            {
                if (item != null)
                    return item;
            }
            return default(T);
        }

        /// <summary>
        /// TryGetValue for a list
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static bool TryGetValue<T>(this IList<T> list, Func<T, bool> match, out T value) where T : class
        {
            for (int i = 0; i < list.Count; i++)
            {
                T obj = list[i];
                if (match(obj))
                {
                    value = obj;
                    return true;
                }
            }
            value = null;
            return false;
        }
    }
}
