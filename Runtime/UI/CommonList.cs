//使用utf-8
using CommonBase;
using JetBrains.Annotations;
using System.Collections;
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

        private void Awake()
        {
            existedList = new List<GameObject>();
            for (int i = 0; i < itemParent.transform.childCount; i++)
            {
                existedList.Add(itemParent.transform.GetChild(i).gameObject);
            }
        }

        public void BindData<T>(T[] data)
        {
            if(data==null || data.Length == 0) { return; }
            for (int i = 0; i < data.Length; i++)
            {
                if (existedList.Count > i)
                {
                    existedList[i].SetActive(true);
                    existedList[i].GetComponent<IListItem<T>>().BindData(data[i]);
                }
                else if (!onlyUseExisted)
                {
                    var curGo = GameObject.Instantiate(itemPrefab, itemParent);
                    if (curGo != null)
                    {
                        curGo.SetActive(true);
                        curGo.GetComponent<IListItem<T>>().BindData(data[i]);
                        existedList.Add(curGo);
                    }
                }
            }

            if (data.Length < existedList.Count)
            {
                for (int i = data.Length; i < existedList.Count; i++)
                {
                    existedList[i].SetActive(false);
                }
            }
        }
    }
}

