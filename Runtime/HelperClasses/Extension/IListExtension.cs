﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

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

        public static List<T> DeepClone<T>(this List<T> desList, List<T> originalLists)
        {
            desList.Clear();
            foreach (var item in originalLists)
            {
                desList.Add(item);
            }
            return desList;
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

        public static T Random<T>(this IList<T> list, out int resultIndex)
        {
            if (list == null || list.Count == 0)
            {
                resultIndex = -1;
                return default;
            }
            resultIndex = UnityEngine.Random.Range(0, list.Count);
            return list[resultIndex];
        }

        public static T Random<T>(this HashSet<T> hashSet)
        {
            if (hashSet == null || hashSet.Count == 0)
            {
                return default;
            }
            return hashSet.ToList().Random();
        }

        public static void Shuffle<T>(this IList<T> list)
        {
            T temp = default;
            int switchItemIndex = 0;
            int listCount = list.Count;

            for (int i = 0; i < list.Count; i++)
            {
                temp = list[i];
                switchItemIndex = UnityEngine.Random.Range(0, listCount);
                list[i] = list[switchItemIndex];
                list[switchItemIndex] = temp;
            }
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

        public static int LoopNext<T>(this int curIndex, IList<T> list)
        {
            if (curIndex < list.Count - 1)
            {
                return curIndex + 1;
            }
            else
            {
                return 0;
            }
        }
    }
}
