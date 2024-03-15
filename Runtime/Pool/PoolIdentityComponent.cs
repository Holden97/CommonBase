using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CommonBase
{
    public class PoolIdentityComponent : MonoBehaviour
    {
        public string poolName;
        public void Init(string poolName)
        {
            this.poolName = poolName;
        }
    }
}
