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
        }

        public void CreatePool(int poolSize, GameObject poolPrefab, string poolName, Transform parent = null)
        {
            if (poolInfo == null)
            {
                poolInfo = new Dictionary<string, Pool>();
            }
            if (poolQueue == null)
            {
                poolQueue = new Dictionary<int, List<PoolItem<GameObject>>>();
            }

            if (poolInfo.ContainsKey(poolName))
            {
                return;
            }
            else
            {
                Pool pool = new Pool(poolSize, poolPrefab);
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
                    Transform realParent = null;
                    if (parent != null)
                    {
                        realParent = parent;
                    }
                    else
                    {
                        realParent = defaultParent;
                    }
                    go = GameObject.Instantiate(poolPrefab, realParent);
                    go.SetActive(false);
                    if (poolQueue.ContainsKey(poolPrefab.GetInstanceID()))
                    {
                        poolQueue[poolPrefab.GetInstanceID()].Add(new PoolItem<GameObject>(go));
                    }
                    else
                    {
                        poolQueue.Add(poolPrefab.GetInstanceID(), new List<PoolItem<GameObject>>());
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

        public GameObject GetNextObject(string key, bool autoRecycle = false)
        {
            if (poolInfo.ContainsKey(key))
            {
                var curPool = poolInfo[key];
                if (poolQueue.ContainsKey(curPool.prefabId))
                {
                    var curGo = poolQueue[curPool.prefabId].Find(x => !x.hasBeenUsed);
                    if (curGo != null)
                    {
                        curGo.poolInstance.SetActive(true);
                        curGo.hasBeenUsed = true;
                        return curGo.poolInstance;
                    }
                    else
                    {
                        var go = GameObject.Instantiate(curPool.poolPrefab, defaultParent);
                        var poolItem = new PoolItem<GameObject>(go);
                        poolQueue[curPool.prefabId].Add(poolItem);
                        poolItem.poolInstance.SetActive(true);
                        poolItem.hasBeenUsed = true;

                        return poolItem.poolInstance;
                    }
                }
                else
                {
                    Debug.LogWarning($"No pool object for {key}");
                    return null;
                }
            }
            else
            {
                return null;
            }
        }

        public void Putback(string key, GameObject curObject)
        {
            if (poolInfo.ContainsKey(key))
            {
                var curPool = poolInfo[key];
                if (poolQueue.ContainsKey(curPool.prefabId))
                {
                    curObject.SetActive(false);
                    var curPoolItem = poolQueue[curPool.prefabId].
                        Find(x =>
                        x.poolInstance.GetInstanceID()
                        == curObject.GetInstanceID());
                    curPoolItem.hasBeenUsed = false;
                }
                else
                {
                    GameObject.Destroy(curObject);
                }
            }

        }

        public void PutbackAll(string key, bool isMainThread = true)
        {
            //只有主线程可以使用SetActive方法
            if (!isMainThread) return;

            if (poolInfo.ContainsKey(key))
            {
                var curPool = poolInfo[key];
                if (poolQueue.ContainsKey(curPool.prefabId))
                {
                    foreach (PoolItem<GameObject> item in poolQueue[curPool.prefabId])
                    {

                        item.poolInstance.gameObject.SetActive(false);
                        item.hasBeenUsed = false;
                    }
                }
            }
        }

        public override void Dispose()
        {
            poolInfo.Clear();
            poolQueue.Clear();
            defaultParent.DestroyChildren();
            base.Dispose();
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
    }

}
