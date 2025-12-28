//使用utf-8

using System.Collections;
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

        /// <summary>
        /// 运行时是否删除预制体中已有的 item（而不是仅仅隐藏它们）
        /// </summary>
        public bool destroyExistedItemsOnRuntime;

        private List<IListItem> existedList;

        private void Reset()
        {
            this.itemParent = this.transform;
        }

        public IEnumerator<IListItem> GetEnumerator()
        {
            foreach (var item in existedList)
            {
                if (item.InUse)
                {
                    yield return item;
                }
                else
                {
                    continue;
                }
            }
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
            if (data == null)
            {
                return;
            }

            if (itemParent == null)
            {
                Debug.LogWarning("You haven't set items' parent.");
                return;
            }

            if (existedList == null)
            {
                existedList = new List<IListItem>();
                //避免嵌套的列表，即一个节点下有多重CommonList，所以只遍历自己的直接子节点
                for (int i = 0; i < itemParent.childCount; i++)
                {
                    Transform childTransform = itemParent.GetChild(i);
                    var listItem = childTransform.GetComponent<IListItem>();
                    if (listItem != null)
                    {
                        existedList.Add(listItem);
                    }
                }

                // 如果开启了运行时删除选项，删除预制体中已有的 item
                if (destroyExistedItemsOnRuntime && Application.isPlaying)
                {
                    for (int i = existedList.Count - 1; i >= 0; i--)
                    {
                        if (existedList[i] is MonoBehaviour m)
                        {
                            Destroy(m.gameObject);
                        }
                    }

                    existedList.Clear();
                }

                // existedList = itemParent.GetComponentsInChildren<IListItem>().ToList();
            }

            for (int i = 0; i < data.Count; i++)
            {
                if (existedList.Count > i)
                {
                    if (!existedList[i].Initialized)
                    {
                        existedList[i].Initialized = true;
                        existedList[i].Initialize(data[i]);
                    }

                    existedList[i].BindData(data[i]);
                    if (existedList[i] is MonoBehaviour m)
                    {
                        m.gameObject.SetActive(true);
                        existedList[i].InUse = true;
                    }
                }
                else if (!onlyUseExisted)
                {
                    var curGo = Instantiate(itemPrefab, itemParent);
                    if (curGo != null)
                    {
                        curGo.SetActive(true);
                        var curItem = curGo.GetComponent<IListItem>();
                        if (curItem == null)
                        {
                            Debug.LogError($"{curGo.name}不包含带有IListItem接口的组件，请检查");
                        }

                        curItem.InUse = true;
                        if (!curItem.Initialized)
                        {
                            curItem.Initialized = true;
                            curItem.Initialize(data[i]);
                        }

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
                        existedList[i].InUse = false;
                    }
                }
            }
        }
    }
}