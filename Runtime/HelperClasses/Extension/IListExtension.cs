using System;
using System.Collections;
using System.Collections.Generic;
using static DG.DemiEditor.DeEditorUtils;

namespace CommonBase
{
    public static class IListExtension
    {
        public static bool ValidIndex(this IList list, int index)
        {
            if (list == null)
            {
                return false;
            }
            return list.Count > index && index >= 0;
        }

        public static bool IsNullOrEmpty(this IList list)
        {
            return list == null || list.Count == 0;
        }

        public static List<T> CustomClone<T>(this List<T> originalLists)
        {
            var list = new List<T>();
            foreach (var item in originalLists)
            {
                list.Add(item);
            }
            return list;
        }

        public static bool NotLastIndex(this int index, IList list)
        {
            if (list == null)
            {
                return false;
            }
            return list.Count - 1 > index;
        }

        public static void CircularAdvance()
        {

        }

        public static T Random<T>(this IList<T> list)
        {
            if (list == null || list.Count == 0)
            {
                return default;
            }
            return list[UnityEngine.Random.Range(0, list.Count)];
        }

        public static List<T> Random<T>(this IList<T> list, int count)
        {
            var result = new List<T>();
            if (list == null || list.Count == 0)
            {
                return default;
            }

            var tempList = new List<T>();
            foreach (var item in list)
            {
                tempList.Add(item);
            }

            for (int i = 0; i < count; i++)
            {
                var current = tempList[UnityEngine.Random.Range(0, tempList.Count)];
                tempList.Remove(current);
                result.Add(current);
            }

            return result;
        }

        public static T RandomOther<T>(this IList<T> list, T current) where T : class
        {
            if (list == null || list.Count == 0)
            {
                return default;
            }

            var tempList = new List<T>();
            foreach (var item in list)
            {
                if (item != current)
                {
                    tempList.Add(item);
                }
            }
            return tempList.Random();
        }

        public static T RandomOtherRange<T>(this IList<T> list, IList<T> current) where T : class
        {
            if (list == null || list.Count == 0)
            {
                return default;
            }

            var tempList = new List<T>();
            foreach (var item in list)
            {
                if (!current.Contains(item))
                {
                    tempList.Add(item);
                }
            }
            return tempList.Random();
        }
    }
}
