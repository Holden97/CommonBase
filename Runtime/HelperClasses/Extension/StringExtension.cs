
using System;
using UnityEngine;

namespace CommonBase
{
    public static class StringExtension
    {
        public static bool IsNullOrEmpty(this string str)
        {
            return str == null || str.Trim() == "";
        }

        public static string ToGreen(this string str)
        {
            return "<color=#00FF00>" + str + "</color>";
        }

        public static string ToRed(this string str)
        {
            return "<color=#FF0000>" + str + "</color>";
        }

        public static string ToRemarkColor(this string str, Func<bool> goodFunc, Func<bool> badFunc = null)
        {
            if (goodFunc != null && goodFunc())
            {
                str = str.ToGreen();
            }
            else if (badFunc != null && badFunc())
            {
                str = str.ToRed();
            }
            return str;
        }

        public static string ToBuffRemarkColor(this float v, float reference)
        {
            if (reference > v)
            {
                return v.ToString().ToRed();
            }
            else if (v > reference)
            {
                return v.ToString().ToGreen();
            }
            return v.ToString();
        }

        public static string ToDebuffRemarkColor(this float v, float reference)
        {
            if (reference < v)
            {
                return v.ToString().ToRed();
            }
            else if (v < reference)
            {
                return v.ToString().ToGreen();
            }
            return v.ToString();
        }

        public static string ToBuffRemarkColor(this int v, float reference)
        {
            if (reference > v)
            {
                return v.ToString().ToRed();
            }
            else if (v > reference)
            {
                return v.ToString().ToGreen();
            }
            return v.ToString();
        }

        public static string ToReverseBuffRemarkColor(this int v, float reference)
        {
            if (reference < v)
            {
                return v.ToString().ToRed();
            }
            else if (v < reference)
            {
                return v.ToString().ToGreen();
            }
            return v.ToString();
        }

        public static string ToDebuffRemarkColor(this int v, float reference)
        {
            if (reference < v)
            {
                return v.ToString().ToRed();
            }
            else if (v < reference)
            {
                return v.ToString().ToGreen();
            }
            return v.ToString();
        }

        public static string ToGrey(this string str)
        {
            return "<color=#8C8C8C>" + str + "</color>";
        }

        public static string ToWhite(this string str)
        {
            return "<color=#FFFFFF>" + str + "</color>";
        }

        public static string ToRemarkValueColor(this string str, float selfValue, float d = 0)
        {
            if (Mathf.Approximately(selfValue, d))
            {
                return str;
            }
            else if (selfValue < d)
            {
                str = str.ToRed();
            }
            else if (selfValue > d)
            {
                str = str.ToGreen();
            }
            return str;
        }
    }
}
