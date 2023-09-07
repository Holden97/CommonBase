using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CommonBase
{
    public class PoolItem<T>
    {
        public T poolInstance;
        public bool hasBeenUsed;

        public PoolItem(T poolInstance, bool hasBeenUsed = false)
        {
            this.poolInstance = poolInstance;
            this.hasBeenUsed = hasBeenUsed;
        }
    }
}

