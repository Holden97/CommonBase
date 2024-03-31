using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CommonBase
{
    public class StackPro<T> : IEnumerable<T> where T : BaseUI
    {
        public int Count => items.Count;
        protected List<T> items = new List<T>();

        public void Push(T baseUI)
        {
            items.Add(baseUI);
            items.Sort(new UIComparer());
            Debug.Log("uisort:--------");
            foreach (var item in items)
            {
                Debug.Log("uiname:" + item.name + ",uilayer" + item.uiLayer.ToString() + ",uisortlayer:" + item.orderInLayer);
            }
        }

        public T Pop()
        {
            if (items.Count > 0)
            {
                T temp = items[items.Count - 1];
                items.RemoveAt(items.Count - 1);
                return temp;
            }
            else
            {
                return default(T);
            }
        }

        public void Remove(int itemIndex)
        {
            items.RemoveAt(itemIndex);
        }

        public void Remove(T ui)
        {
            items.Remove(ui);
        }

        public IEnumerator<T> GetEnumerator()
        {
            return new StackProEnumerator(items);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        private class StackProEnumerator : IEnumerator<T>
        {
            private List<T> _items;
            private int _index = -1;

            public StackProEnumerator(List<T> items)
            {
                _items = items;
            }

            public T Current => _items[_index];

            object IEnumerator.Current => Current;

            public void Dispose()
            {
                //后续补充？
            }

            public bool MoveNext()
            {
                _index++;
                return _index < _items.Count;
            }

            public void Reset()
            {
                _index = -1;
            }
        }

        public T this[int index]
        {
            get
            {
                if (index >= 0 && index < items.Count)
                {
                    return items[index];
                }
                else
                {
                    throw new IndexOutOfRangeException("Index is out of range.");
                }
            }

            set
            {
                if (index >= 0 && index < items.Count)
                {
                    items[index] = value;
                }
                else
                {
                    throw new IndexOutOfRangeException("Index is out of range.");
                }
            }
        }
    }

    class UIComparer : IComparer<BaseUI>
    {

        public UIComparer()
        {
        }

        /// <summary>
        /// 自定义距离排序器
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public int Compare(BaseUI x, BaseUI y)
        {
            if (x.uiLayer != y.uiLayer)
            {
                return x.uiLayer.CompareTo(y.uiLayer);
            }
            else
            {
                return x.orderInLayer.CompareTo(y.orderInLayer);
            }
        }
    }

}
