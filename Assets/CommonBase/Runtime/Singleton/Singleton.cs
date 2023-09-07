using System;
using System.Reflection;
using UnityEngine;

namespace CommonBase
{
    public class Singleton<T> : IListener where T : Singleton<T>
    {
        private static T instance;

        public static T Instance
        {
            get
            {
                if (instance == null)
                {
                    var type = typeof(T);
                    var ctors = type.GetConstructors(BindingFlags.Instance | BindingFlags.NonPublic);
                    var ctor = Array.Find(ctors, c => c.GetParameters().Length == 0);

                    if (ctor == null)
                    {
                        throw new Exception("没有找到私有的非静态构造函数:" + type.Name);
                    }

                    instance = ctor.Invoke(null) as T;
                    Debug.Log($"创建{type}管理器实例！");
                    instance.OnCreateInstance();
                }
                return instance;
            }
        }

        public virtual void Tick()
        {

        }

        public virtual void OnCreateInstance()
        {

        }

        public virtual void DisposeAll()
        {
            instance = null;
            this.EventUnregister();
        }
    }

}
