using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CommonBase
{
    public static class ArrayExtension
    {
        public static T Find<T>(this T[] array, Predicate<T> assertion)
        {
            for (int i = 0; i < array.Length; i++)
            {
                if (assertion(array[i]))
                {
                    return array[i];
                }
            }
            return default;
        }

        public static List<T> FindAll<T>(this T[] array, Predicate<T> assertion)
        {
            var list = new List<T>();
            for (int i = 0; i < array.Length; i++)
            {
                if (assertion(array[i]))
                {
                    list.Add(array[i]);
                }
            }
            return list;
        }

        public static void Remove<T>(this T[] array, T element) where T : class
        {
            for (int i = 0; i < array.Length; i++)
            {
                if (element == array[i])
                {
                    array[i] = null;
                    break;
                }
            }
        }

        public static void Add<T>(this T[] array, T element) where T : class
        {
            for (int i = 0; i < array.Length; i++)
            {
                if (array[i] == null)
                {
                    array[i] = element;
                    break;
                }
            }
        }

        public static int IndexOf<T>(this T[] array, T element) where T : class
        {
            for (int i = 0; i < array.Length; i++)
            {
                if (element == array[i])
                {
                    return i;
                }
            }
            return -1;
        }

        public static bool IsFull<T>(this T[] array) where T : class
        {
            for (int i = 0; i < array.Length; i++)
            {
                if (array[i] == null)
                {
                    return false;
                }
            }
            return true;
        }

        public static int FirstEmpty<T>(this T[] array) where T : class
        {
            for (int i = 0; i < array.Length; i++)
            {
                if (array[i] == null)
                {
                    return i;
                }
            }
            return -1;
        }
    }
}
