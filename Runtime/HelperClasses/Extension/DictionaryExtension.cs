using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace CommonBase
{
    public static class DictionaryExtension
    {
        public static List<TValue> FindAll<TKey, TValue>(this Dictionary<TKey, TValue> dictionary, Func<TValue, bool> predicate)
        {
            List<TValue> res = new List<TValue>();
            foreach (var kvp in dictionary)
            {
                if (predicate(kvp.Value))
                {
                    res.Add(kvp.Value);
                }
            }
            return res;
        }

        public static TValue Find<TKey, TValue>(this Dictionary<TKey, TValue> dictionary, Func<TValue, bool> predicate)
        {
            foreach (var kvp in dictionary)
            {
                if (predicate(kvp.Value))
                {
                    return kvp.Value;
                }
            }
            // 或者根据具体情况返回合适的默认值
            return default;
        }

        /// <summary>
        /// Works like List.RemoveAll.
        /// </summary>
        /// <typeparam name="TKey">Key type</typeparam>
        /// <typeparam name="TValue">Value type</typeparam>
        /// <param name="dictionary">Dictionary to remove entries from</param>
        /// <param name="match">Delegate to match keys</param>
        /// <returns>Number of entries removed</returns>
        public static int RemoveAllByKey<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, Predicate<TKey> match, Action<TKey> onRemove = null)
        {
            if (dictionary == null || match == null) return 0;
            var keysToRemove = dictionary.Keys.Where(k => match(k)).ToList();
            if (keysToRemove.Count > 0)
            {
                foreach (var key in keysToRemove)
                {
                    onRemove?.Invoke(key);
                    dictionary.Remove(key);
                }
            }
            return keysToRemove.Count;
        }

        /// <summary>
        /// Works like List.RemoveAll.
        /// </summary>
        /// <typeparam name="TKey">Key type</typeparam>
        /// <typeparam name="TValue">Value type</typeparam>
        /// <param name="dictionary">Dictionary to remove entries from</param>
        /// <param name="match">Delegate to match keys</param>
        /// <returns>Number of entries removed</returns>
        public static void RemoveAllByValue<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, Predicate<TValue> match)
        {
            if (dictionary == null || match == null)
                return;
            var keys = dictionary.Keys.ToList();

            for (var i = 0; i < keys.Count; i++)
            {
                if (match(dictionary[keys[i]]))
                {
                    dictionary.Remove(keys[i]);
                }
            }
        }
    }
}
