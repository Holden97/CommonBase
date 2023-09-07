using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CommonBase
{
    public static class OverlapHelper
    {
        /// <summary>
        /// 判断矩形范围内是否含有T类型组件
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="listComponentsAtBoxPosition"></param>
        /// <param name="point"></param>
        /// <param name="size"></param>
        /// <param name="angle"></param>
        /// <returns></returns>
        public static bool GetComponentsAtBoxLocation<T>(out List<T> listComponentsAtBoxPosition, Vector2 point, Vector2 size, float angle)
        {
            bool found = false;
            List<T> listComponents = new List<T>();

            Collider2D[] colliders = Physics2D.OverlapBoxAll(point, size, angle);

            for (int i = 0; i < colliders.Length; i++)
            {
                //我认为这一段中两个Add有可能重复添加同一个组件
                T tComponent = colliders[i].gameObject.GetComponentInParent<T>();
                if (tComponent != null)
                {
                    found = true;
                    listComponents.Add(tComponent);
                }
                else
                {
                    tComponent = colliders[i].gameObject.GetComponentInChildren<T>();
                    if (tComponent != null)
                    {
                        found = true;
                        listComponents.Add(tComponent);
                    }
                }
            }

            listComponentsAtBoxPosition = listComponents;

            return found;
        }

        /// <summary>
        /// 判断矩形范围内是否含有T类型组件(不分配内存)
        /// 不分配内存而使用自己指定的变量会产生有效数据居中，而两边数据为null的情况。
        /// 如给予一个长度为5的数组存储数据，那么获得的结果有可能为{null,4,null,null,null}，有效数据只有一个，且下标为1
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="numberOfCollidersToTest"></param>
        /// <param name="point"></param>
        /// <param name="size"></param>
        /// <param name="angle"></param>
        /// <returns></returns>
        public static T[] GetComponentsAtBoxLocationNonAlloc<T>(int numberOfCollidersToTest, Vector2 point, Vector2 size, float angle)
        {
            Collider2D[] collider2DArray = new Collider2D[numberOfCollidersToTest];
            Physics2D.OverlapBoxNonAlloc(point, size, angle, collider2DArray);

            T tComponent = default(T);

            T[] componentArray = new T[collider2DArray.Length];

            for (int i = collider2DArray.Length - 1; i >= 0; i--)
            {
                if (collider2DArray[i] != null)
                {
                    tComponent = collider2DArray[i].gameObject.GetComponentInParent<T>();
                    if (tComponent != null)
                    {
                        componentArray[i] = tComponent;
                    }
                }
            }

            return componentArray;
        }

        /// <summary>
        /// 判断鼠标点位置是否含有T类型组件
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="componentsAtPositionList">对应的世界坐标</param>
        /// <param name="worldPositionToCheck"></param>
        /// <returns></returns>
        public static bool GetComponentsAtCursorLocation<T>(out T[] componentsAtPositionList, Vector3 worldPositionToCheck)
        {
            bool found = false;

            List<T> componentList = new List<T>();

            Collider2D[] collider2DArray = Physics2D.OverlapPointAll(worldPositionToCheck);
            T tComponent = default(T);

            for (int i = 0; i < collider2DArray.Length; i++)
            {
                tComponent = collider2DArray[i].gameObject.GetComponentInParent<T>();
                if (tComponent != null)
                {
                    found = true;
                    componentList.Add(tComponent);
                }
                else
                {
                    tComponent = collider2DArray[i].gameObject.GetComponentInChildren<T>();
                    if (tComponent != null)
                    {
                        found = true;
                        componentList.Add(tComponent);
                    }
                }
            }

            componentsAtPositionList = componentList.ToArray();

            return found;
        }

        public static bool ScreenOverlapBound(this Rect a, Bounds b)
        {
            var minPoint = Camera.main.WorldToScreenPoint(b.min);
            var maxPoint = Camera.main.WorldToScreenPoint(b.max);
            var Rectb = new Rect(minPoint, maxPoint - minPoint);

            return a.Overlaps(Rectb);
        }

        public static bool ScreenContainsWorldPos(this Rect a, Vector3 b)
        {
            return a.Contains(Camera.main.WorldToScreenPoint(b));
        }

        public static bool ScreenContainsWorldIntPos(this Rect a, Vector3Int b)
        {
            return a.ScreenContainsWorldPos((Vector3)b);
        }
    }

}
