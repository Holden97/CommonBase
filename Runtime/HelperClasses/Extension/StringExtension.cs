using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CommonBase
{
    public static class StringExtension
    {
        public static bool IsNullOrEmpty(this string str)
        {
            return str == null || str.Trim() == "";
        }
    }
}
