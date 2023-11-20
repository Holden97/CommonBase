using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CommonBase
{
    public static class ICollectionExtensions
    {
        public static bool IsNullOrEmpty(this ICollection list)
        {
            return list == null || list.Count == 0;
        }
    }
}
