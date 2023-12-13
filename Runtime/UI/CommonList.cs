//使用utf-8
using System.Collections.Generic;
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

        private List<GameObject> existedList;

        private void Reset()
        {
            this.itemParent = this.transform;
        }

        private void Awake()
        {
            existedList = new List<GameObject>();
            if (itemParent == null) return;
            for (int i = 0; i < itemParent.transform.childCount; i++)
            {
                existedList.Add(itemParent.transform.GetChild(i).gameObject);
            }
        }

        public void BindData<T>(IList<T> data)
        {
            if (data == null) { return; }
            for (int i = 0; i < data.Count; i++)
            {
                if (existedList.Count > i)
                {
                    existedList[i].SetActive(true);
                    existedList[i].GetComponent<IListItem>().BindData(data[i]);
                }
                else if (!onlyUseExisted)
                {
                    var curGo = Instantiate(itemPrefab, itemParent);
                    if (curGo != null)
                    {
                        curGo.SetActive(true);
                        curGo.GetComponent<IListItem>().BindData(data[i]);
                        existedList.Add(curGo);
                    }
                }
            }

            if (data.Count < existedList.Count)
            {
                for (int i = data.Count; i < existedList.Count; i++)
                {
                    existedList[i].SetActive(false);
                }
            }
        }
    }
}

