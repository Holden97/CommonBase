//使用utf-8
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CommonBase
{
    public interface IUIList
    {
        public GameObject ItemPrefab { get; }
        public void BindData<T>(IList<T> data);
    }
}

