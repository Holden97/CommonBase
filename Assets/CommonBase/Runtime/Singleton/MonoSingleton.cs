using System.Collections.Generic;
using UnityEngine;


namespace CommonBase
{
    public abstract class MonoSingleton<T> : MonoBehaviour, IListener where T : MonoBehaviour
    {
        public static Dictionary<string, object> MonoSingletons = new Dictionary<string, object>();
        private static T instance;
        protected static bool AppIsQuit;
        private static bool IsDirty = true;
        public static T Instance
        {
            get
            {
                if (AppIsQuit)
                {
                    return null;
                }
                if (instance == null)
                {
                    instance = FindObjectOfType<T>();

                    if (instance != null)
                    {
                        if (IsDirty)
                        {
                            MonoSingletons.Add(instance.name, instance);
                            IsDirty = false;
                        }
                    }
                }

                return instance;
            }
        }

        public virtual void Initialize()
        {
            if (instance == null)
            {
                instance = FindObjectOfType<T>();
                if (instance == null)
                {
                    var go = new GameObject($"[MonoSingleton]{typeof(T).Name}");
                    instance = go.AddComponent<T>();
                }
                DontDestroyOnLoad(instance);
            }
        }

        /// <summary>
        /// 已经拥有实例
        /// </summary>
        public static bool hasInstance
        {
            get { return instance != null; }
        }

        protected virtual void Awake()
        {
            if (instance == null)
            {
                //如果使用instance = this as T;会遇到this挂载的gameobject如果同时挂载了别的脚本，as关键字无法分析this类型的情况
                instance = this.gameObject.GetComponent<T>();
                AppIsQuit = false;
                //非根节点DontDestroyOnLoad无效
                transform.SetParent(null);
                DontDestroyOnLoad(instance);
            }
            else
            {
                if (instance != this)
                {
                    Debug.LogError($"已经有{typeof(T).Name}的单例示例,新生成的示例已被删除。");
                    Destroy(gameObject);
                }
            }
        }

        private void OnApplicationQuit()
        {
            AppIsQuit = true;
        }

        protected virtual void OnDisable()
        {
            this.EventUnregister();
        }
    }
}
