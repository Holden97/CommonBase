using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CommonBase
{
    public class AutoListSO<T> : ScriptableObject
    {
        public List<T> info;
    }
}
