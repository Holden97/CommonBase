using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CommonBase
{
    public abstract class MonoListItem : MonoBehaviour, IListItem
    {
        public bool InUse { get; set; }

        public abstract void BindData(object data);
    }
}
