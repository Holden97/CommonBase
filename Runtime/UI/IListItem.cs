//使用utf-8
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CommonBase
{
    public interface IListItem<T>
    {
        public T ItemInfo { get; }
        void BindData(T data);
    }
}


