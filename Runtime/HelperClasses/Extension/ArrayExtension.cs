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

        public static T FindNotNull<T>(this T[] array, Predicate<T> assertion)
        {
            for (int i = 0; i < array.Length; i++)
            {
                if (array[i] == null)
                {
                    continue;
                }
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

        public static T Random<T>(this T[,] array) where T : class
        {
            var x = UnityEngine.Random.Range(0, array.GetLength(0));
            var y = UnityEngine.Random.Range(0, array.GetLength(1));
            return array[x, y];
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

        public static List<T> Find<T>(this T[,] array, Func<T, bool> predict)
        {
            var list = new List<T>();
            for (int i = 0; i < array.GetLength(0); i++)
            {
                for (int j = 0; j < array.GetLength(1); j++)
                {
                    if (predict.Invoke(array[i, j]))
                    {
                        list.Add(array[i, j]);
                    }
                }
            }
            return list;
        }

        public static T FindFirst<T>(this T[,] array, Func<T, bool> predict)
        {
            for (int i = 0; i < array.GetLength(0); i++)
            {
                for (int j = 0; j < array.GetLength(1); j++)
                {
                    if (predict.Invoke(array[i, j]))
                    {
                        return array[i, j];
                    }
                }
            }
            return default;
        }

        public static bool TryGet<T>(this T[,] array, int x, int y, out T result)
        {
            var list = new List<T>();
            if (x < 0 || y < 0 || x >= array.GetLength(0) || y >= array.GetLength(1))
            {
                result = default(T);
                return false;
            }
            else
            {
                result = array[x, y];
                return true;
            }
        }
    }
}
