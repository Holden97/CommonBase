using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CommonBase
{
    public struct Pool
    {
        public int prefabId;
        public string prefabName;
        public int poolSize;
        public GameObject poolPrefab;
        /// <summary>
        /// 在加载新场景时不销毁
        /// </summary>
        public bool dontDestroyOnload;

        public Pool(int prefabId, string prefabName, int poolSize, GameObject poolPrefab, bool dontDestroyOnload)
        {
            this.prefabId = prefabId;
            this.prefabName = prefabName;
            this.poolSize = poolSize;
            this.poolPrefab = poolPrefab;
            this.dontDestroyOnload = dontDestroyOnload;
        }
    }
}

