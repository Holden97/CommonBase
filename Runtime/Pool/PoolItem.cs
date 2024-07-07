using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CommonBase
{
    public class PoolItem<T>
    {
        public T poolInstance;
        public bool hasBeenUsed;
        public Transform parent;

        public PoolItem(T poolInstance, bool hasBeenUsed, Transform parent)
        {
            this.poolInstance = poolInstance;
            this.hasBeenUsed = hasBeenUsed;
            this.parent = parent;
        }
    }
}

