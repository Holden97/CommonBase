using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace CommonBase
{
    public class ObjectPoolManager : Singleton<ObjectPoolManager>
    {
        /// <summary>
        /// 对象池配置信息
        /// </summary>
        private Dictionary<string, Pool> poolInfo;
        /// <summary>
        /// 对象池队列,使用List是因为Queue在元素出栈后不好管理
        /// </summary>
        private Dictionary<int, List<PoolItem<GameObject>>> poolQueue;
        /// <summary>
        /// 默认父节点
        /// </summary>
        private Transform defaultParent;

        private ObjectPoolManager()
        {
            poolInfo = new Dictionary<string, Pool>();
            poolQueue = new Dictionary<int, List<PoolItem<GameObject>>>();
        }

        public bool Contains(string poolName)
        {
            if (poolInfo == null) { return false; }
            return poolInfo.ContainsKey(poolName);
        }

        public void CreatePool(string poolName, GameObject poolPrefab, int poolSize, Transform parent = null, bool dontDestroyOnLoad = false, Action<GameObject> OnCreate = null)
        {
            if (poolInfo.ContainsKey(poolName))
            {
                return;
            }
            else
            {
                Pool pool = new Pool(poolPrefab.GetInstanceID(), poolPrefab.name, poolSize, poolPrefab, dontDestroyOnLoad);
                if (poolName != null)
                {
                    poolInfo.Add(poolName, pool);
                }
                else
                {
                    poolInfo.Add(poolPrefab.GetInstanceID().ToString(), pool);
                }

                for (int i = 0; i < pool.poolSize; i++)
                {
                    GameObject go;
                    Transform actualParent = null;
                    if (parent != null)
                    {
                        actualParent = parent;
                    }
                    else
                    {
                        actualParent = defaultParent;
                    }
                    go = GameObject.Instantiate(poolPrefab, actualParent);
                    go.AddComponent<PoolIdentityComponent>().Init(poolName);
#if UNITY_EDITOR
                    go.gameObject.name = $"{poolName}_{go.GetInstanceID()}";
#endif
                    OnCreate?.Invoke(go);
                    go.GetOrAddComponent<PoolIdentityComponent>().Init(poolName);
                    go.SetActive(false);
                    if (poolQueue.ContainsKey(poolPrefab.GetInstanceID()))
                    {
                        poolQueue[poolPrefab.GetInstanceID()].Add(new PoolItem<GameObject>(go, false, actualParent));
                    }
                    else
                    {
                        poolQueue.Add(poolPrefab.GetInstanceID(), new List<PoolItem<GameObject>>() { new PoolItem<GameObject>(go, false, actualParent) });
                    }

                }
            }
        }

        public PoolItem<GameObject> Find(Predicate<PoolItem<GameObject>> match)
        {
            var allGameobjects = new List<PoolItem<GameObject>>();
            var values = poolQueue.Values;
            var valueList = values.ToList();
            foreach (var item in valueList)
            {
                allGameobjects.AddRange(item);
            }
            return allGameobjects.Find(match);
        }

        public void PutBackOthers(string key, List<GameObject> usingObject)
        {
            if (poolInfo.ContainsKey(key))
            {
                var curPool = poolInfo[key];
                if (usingObject.Count >= poolQueue[curPool.prefabId].Count)
                {
                    return;
                }
                if (poolQueue.ContainsKey(curPool.prefabId))
                {
                    foreach (var item in poolQueue[curPool.prefabId])
                    {
                        if (usingObject.Contains(item.poolInstance))
                        {
                            continue;
                        }
                        else
                        {
                            Putback(key, item.poolInstance);
                        }
                    }
                }
            }
        }

        public GameObject GetNextObject(string key, bool autoActive = true)
        {
            if (poolInfo.ContainsKey(key))
            {
                var curPool = poolInfo[key];
                if (poolQueue.ContainsKey(curPool.prefabId))
                {
                    var curGo = poolQueue[curPool.prefabId].Find(x => !x.hasBeenUsed);
                    if (curGo != null)
                    {
                        curGo.poolInstance.SetActive(autoActive);
                        curGo.hasBeenUsed = true;
                        return curGo.poolInstance;
                    }
                    else
                    {
                        var go = GameObject.Instantiate(curPool.poolPrefab, defaultParent);
                        go.AddComponent<PoolIdentityComponent>().Init(key);
                        var poolItem = new PoolItem<GameObject>(go, false, defaultParent);
                        poolQueue[curPool.prefabId].Add(poolItem);
                        poolItem.poolInstance.SetActive(true);
                        poolItem.hasBeenUsed = true;

                        return poolItem.poolInstance;
                    }
                }
                else
                {
                    Debug.LogWarning($"未创建键值为{key}的对象池");
                    return null;
                }
            }
            else
            {
                Debug.LogWarning($"未找到键值为{key}的对象池信息配置!");
                return null;
            }
        }

        public void Putback(string key, GameObject curObject, bool active = false, Vector3 pos = default)
        {
            if (poolInfo.ContainsKey(key))
            {
                var curPool = poolInfo[key];
                if (poolQueue.ContainsKey(curPool.prefabId))
                {
                    if (!active)
                    {
                        curObject.SetActive(false);
                    }
                    else
                    {
                        //不设置为false，而是放到指定位置
                        curObject.transform.position = pos;
                    }
                    var curPoolItem = poolQueue[curPool.prefabId].
                        Find(x =>
                        x.poolInstance.GetInstanceID()
                        == curObject.GetInstanceID());
                    if (curPoolItem == null)
                    {
                        GameObject.Destroy(curObject);
                        Debug.LogWarning("对象并不是从名为" + key + "的对象池中取出，已销毁对象，请检查");
                    }
                    else
                    {
                        curPoolItem.hasBeenUsed = false;
                    }
                }
                else
                {
                    GameObject.Destroy(curObject);
                }
            }
            else
            {
                GameObject.Destroy(curObject);
                Debug.LogWarning("未创建名称为" + key + "的对象池，已销毁对象，请检查");
            }

        }

        public void PutbackAll(string key, bool active = false, Vector3 pos = default)
        {
            if (key == null) return;
            if (poolInfo.ContainsKey(key))
            {
                var curPool = poolInfo[key];
                if (poolQueue.ContainsKey(curPool.prefabId))
                {
                    foreach (PoolItem<GameObject> item in poolQueue[curPool.prefabId])
                    {
                        if (!active)
                        {
                            item.poolInstance.gameObject.SetActive(active);
                        }
                        else
                        {
                            item.poolInstance.gameObject.transform.position = pos;
                        }
                        item.poolInstance.transform.SetParent(item.parent);
                        item.hasBeenUsed = false;
                    }
                }
            }
        }

        public IEnumerable<GameObject> GetAllActiveObjects(string key)
        {
            if (poolInfo.ContainsKey(key))
            {
                var curPool = poolInfo[key];
                if (poolQueue.ContainsKey(curPool.prefabId))
                {
                    foreach (PoolItem<GameObject> item in poolQueue[curPool.prefabId])
                    {

                        if (item.hasBeenUsed)
                        {
                            yield return item.poolInstance.gameObject;
                        }
                    }
                }
            }
        }

        public int GetAllActiveObjectsCount(string key)
        {
            var result = 0;
            if (poolInfo.ContainsKey(key))
            {
                var curPool = poolInfo[key];
                if (poolQueue.ContainsKey(curPool.prefabId))
                {
                    foreach (PoolItem<GameObject> item in poolQueue[curPool.prefabId])
                    {

                        if (item.hasBeenUsed)
                        {
                            result++;
                        }
                    }
                }
            }
            return result;
        }


        public override void Dispose()
        {
            //poolInfo.Clear();
            foreach (var item in poolInfo)
            {
                if (!item.Value.dontDestroyOnload)
                {
                    poolQueue.Remove(item.Value.prefabId);
                    defaultParent.DestroyChildren(item.Value.prefabName);
                }
            }
            //poolQueue.Clear();
            //base.Dispose();
            //instance = null;
            //this.EventUnregister();
        }


        public override void OnCreateInstance()
        {
            base.OnCreateInstance();

            defaultParent = new GameObject().transform;
            defaultParent.name = "defaultPoolParent";
            GameObject.DontDestroyOnLoad(defaultParent);
        }

        public void DisposePool(string key)
        {
            if (poolInfo.ContainsKey(key))
            {
                var curPool = poolInfo[key];
                if (poolQueue.ContainsKey(curPool.prefabId))
                {
                    foreach (PoolItem<GameObject> item in poolQueue[curPool.prefabId])
                    {
                        GameObject.Destroy(item.poolInstance.gameObject);
                    }
                }
                poolInfo.Remove(key);
                poolQueue.Remove(curPool.prefabId);
            }
        }

        public bool ContainsPool(string key)
        {
            return poolInfo.ContainsKey(key);
        }
    }

}
