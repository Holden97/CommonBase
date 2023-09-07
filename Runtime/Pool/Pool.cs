using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CommonBase
{
    public struct Pool
    {
        public int prefabId;
        public int poolSize;
        public GameObject poolPrefab;

        public Pool(int poolSize, GameObject poolPrefab)
        {
            this.poolSize = poolSize;
            this.poolPrefab = poolPrefab;
            this.prefabId = poolPrefab.GetInstanceID();
        }
    }
}

