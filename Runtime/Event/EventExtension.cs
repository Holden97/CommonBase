using UnityEngine;
using UnityEngine.Events;

namespace CommonBase
{
    public static class EventExtension
    {
        public static void EventRegister(this IListener caller, string name, UnityAction action)
        {
            Debug.Log($"事件注册:{name}，监听人:{caller},行为名称:{action.Method.Name}");
            EventCenter.Instance.Register(name, action, caller);
        }

        public static void EventRegister<T>(this IListener caller, string name, UnityAction<T> action)
        {
            EventCenter.Instance.Register(name, action, caller);
        }

        public static void EventRegister<T1, T2>(this IListener caller, string name, UnityAction<T1, T2> action)
        {
            EventCenter.Instance.Register(name, action, caller);
        }

        public static void EventUnregisterAll(this IListener listener)
        {
            EventCenter.Instance.EventUnregister(listener);
        }

        public static void EventTrigger(this object trigger, string eventName)
        {
            EventCenter.Instance.Trigger(eventName);
        }

        /// <summary>
        /// 只会触发特定监听者的回调
        /// </summary>
        /// <param name="trigger"></param>
        /// <param name="eventName"></param>
        public static void SpecificEventTrigger(this object trigger, string eventName, object listener)
        {
            EventCenter.Instance.Trigger(eventName, listener);
        }
        public static void SpecificEventTrigger<T>(this object trigger, string eventName, T param, object listener)
        {
            EventCenter.Instance.Trigger(eventName, param, listener);
        }

        public static void SpecificEventTrigger<T1, T2>(this object trigger, string eventName, T1 p1, T2 p2, object listener)
        {
            EventCenter.Instance.Trigger(eventName, p1, p2, listener);
        }

        public static void EventTrigger<T>(this object trigger, string eventName, T param)
        {
            EventCenter.Instance.Trigger(eventName, param);
        }

        public static void EventTrigger<T1, T2>(this object trigger, string eventName, T1 p1, T2 p2)
        {
            EventCenter.Instance.Trigger(eventName, p1, p2);
        }

        public static void EventUnregister(this IListener caller, string name)
        {
            EventCenter.Instance.Unregister(name, caller);
        }
    }
}

