using System;
using System.Collections;
using System.Collections.Generic;
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
    }
}
