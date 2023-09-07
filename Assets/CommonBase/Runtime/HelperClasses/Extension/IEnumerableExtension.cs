using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CommonBase
{
    public static class IEnumerableExtension
    {
        public static IEnumerable<T> Safe<T>(this IEnumerable<T> source)
        {
            if (source == null)
            {
                yield break;
            }

            foreach (var item in source)
            {
                yield return item;
            }
        }
    }
}
