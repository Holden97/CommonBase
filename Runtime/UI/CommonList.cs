//使用utf-8
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace CommonBase
{
    public class CommonList : MonoBehaviour, IUIList
    {
        public GameObject ItemPrefab => itemPrefab;
        public GameObject itemPrefab;
        public Transform itemParent;
        /// <summary>
        /// 是否只使用列表中已存在的预设
        /// </summary>
        public bool onlyUseExisted;

        private List<IListItem> existedList;

        private void Reset()
        {
            this.itemParent = this.transform;
        }

        public void Clean()
        {
            if (existedList == null) return;
            for (int i = 0; i < existedList.Count; i++)
            {
                if (existedList[i] is MonoBehaviour m)
                {
                    m.gameObject.SetActive(false);
                }
            }
        }

        public void BindData<T>(IList<T> data)
        {
            if (data == null) { return; }
            if (itemParent == null)
            {
                Debug.LogWarning("You haven't set items' parent.");
                return;
            }
            if (existedList == null)
            {
                existedList = itemParent.GetComponentsInChildren<IListItem>().ToList();
            }
            for (int i = 0; i < data.Count; i++)
            {
                if (existedList.Count > i)
                {
                    existedList[i].BindData(data[i]);
                    if (existedList[i] is MonoBehaviour m)
                    {
                        m.gameObject.SetActive(true);
                    }
                }
                else if (!onlyUseExisted)
                {
                    var curGo = Instantiate(itemPrefab, itemParent);
                    if (curGo != null)
                    {
                        curGo.SetActive(true);
                        var curItem = curGo.GetComponent<IListItem>();
                        curItem.BindData(data[i]);
                        existedList.Add(curItem);
                    }
                }
            }

            if (data.Count < existedList.Count)
            {
                for (int i = data.Count; i < existedList.Count; i++)
                {
                    if (existedList[i] is MonoBehaviour m)
                    {
                        m.gameObject.SetActive(false);
                    }
                }
            }
        }
    }
}

