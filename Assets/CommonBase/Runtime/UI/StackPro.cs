using System;
using System.Collections;
using System.Collections.Generic;

namespace CommonBase
{
    public class StackPro<T> : IEnumerable<T>
    {
        public int Count => items.Count;
        private List<T> items = new List<T>();

        public void Push(T baseUI)
        {
            items.Add(baseUI);
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
                //ºóÐø²¹³ä£¿
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

}
