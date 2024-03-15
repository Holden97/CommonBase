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
                    OnCreate?.Invoke(go);
                    go.GetOrAddComponent<PoolIdentityComponent>().Init(poolName);
                    go.SetActive(false);
                    if (poolQueue.ContainsKey(poolPrefab.GetInstanceID()))
                    {
                        poolQueue[poolPrefab.GetInstanceID()].Add(new PoolItem<GameObject>(go));
                    }
                    else
                    {
                        poolQueue.Add(poolPrefab.GetInstanceID(), new List<PoolItem<GameObject>>() { new PoolItem<GameObject>(go) });
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
                    Debug.LogError($"未创建键值为{key}的对象池");
                    return null;
                }
            }
            else
            {
                Debug.LogError($"未找到键值为{key}的对象池信息配置!");
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

        public GameObject Instantiate(GameObject prefab, int size = 20)
        {
            CreatePool(prefab.name, prefab, size);
            return GetNextObject(prefab.name);
        }

        public void Destroy(GameObject intance)
        {

        }
    }

}
