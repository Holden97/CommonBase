using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace CommonBase
{

    public interface IListener
    {
    }

    public interface ITrigger
    {
    }

    public interface IEventInfo
    {
        void RemoveListener(IListener listener);
    }

    public class EventInfo<T> : IEventInfo
    {
        public UnityAction<T> actions;
        public Dictionary<object, UnityAction<T>> objectEventDic;

        public EventInfo(UnityAction<T> action, object caller)
        {
            objectEventDic = new Dictionary<object, UnityAction<T>>();

            AddEventInDic(action, caller);
        }

        public void AddEventInDic(UnityAction<T> action, object caller)
        {
            this.actions += action;
            if (objectEventDic.TryGetValue(caller, out var actions))
            {
                objectEventDic[caller] += action;
            }
            else
            {
                objectEventDic[caller] = action;
            }
        }

        public void RemoveListener(IListener caller)
        {
            objectEventDic.TryGetValue(caller, out var callerActions);
            objectEventDic.Remove(caller);
            this.actions -= callerActions;
        }
    }

    public class EventInfo<T1, T2> : IEventInfo
    {
        public UnityAction<T1, T2> actions;
        public Dictionary<object, UnityAction<T1, T2>> objectEventDic;

        public EventInfo(UnityAction<T1, T2> action, object caller)
        {
            this.objectEventDic = new Dictionary<object, UnityAction<T1, T2>>();
            AddEventInDic(action, caller);
        }

        public void AddEventInDic(UnityAction<T1, T2> action, object caller)
        {
            this.actions += action;
            if (objectEventDic.TryGetValue(caller, out var actions))
            {
                objectEventDic[caller] += action;
            }
            else
            {
                objectEventDic[caller] = action;
            }
        }

        public void RemoveListener(IListener caller)
        {
            objectEventDic.TryGetValue(caller, out var callerActions);
            objectEventDic.Remove(caller);
            this.actions -= callerActions;
        }
    }


    public class EventInfo : IEventInfo
    {
        public UnityAction actions;
        public Dictionary<object, UnityAction> objectEventDic;

        public EventInfo(UnityAction actions, object caller)
        {
            this.objectEventDic = new Dictionary<object, UnityAction>();
            AddEventInDic(actions, caller);
        }

        public void AddEventInDic(UnityAction action, object caller)
        {
            this.actions += action;
            if (objectEventDic.TryGetValue(caller, out var actions))
            {
                objectEventDic[caller] += action;
            }
            else
            {
                objectEventDic[caller] = action;
            }
        }

        public void RemoveListener(IListener caller)
        {
            objectEventDic.TryGetValue(caller, out var callerActions);
            objectEventDic.Remove(caller);
            this.actions -= callerActions;
        }
    }

    public class EventCenter : Singleton<EventCenter>
    {
        private Dictionary<string, IEventInfo> eventDic = new Dictionary<string, IEventInfo>();

        public void Register<T1, T2>(string name, UnityAction<T1, T2> action, object caller)
        {
            if (eventDic.ContainsKey(name))
            {
                (eventDic[name] as EventInfo<T1, T2>).AddEventInDic(action, caller);
            }
            else
            {
                eventDic.Add(name, new EventInfo<T1, T2>(action, caller));
            }
        }

        public void Register<T>(string name, UnityAction<T> action, object caller)
        {
            if (eventDic.ContainsKey(name))
            {
                (eventDic[name] as EventInfo<T>).AddEventInDic(action, caller);
            }
            else
            {
                eventDic.Add(name, new EventInfo<T>(action, caller));
            }
        }

        public void Register(string name, UnityAction action, object caller)
        {
            if (eventDic.ContainsKey(name))
            {
                (eventDic[name] as EventInfo).AddEventInDic(action, caller);
            }
            else
            {
                eventDic.Add(name, new EventInfo(action, caller));
            }
        }

        public void Unregister<T>(string name, UnityAction<T> action)
        {
            if (eventDic.ContainsKey(name))
            {
                (eventDic[name] as EventInfo<T>).actions -= action;
            }
        }

        public void Unregister<T1, T2>(string name, UnityAction<T1, T2> action)
        {
            if (eventDic.ContainsKey(name))
            {
                (eventDic[name] as EventInfo<T1, T2>).actions -= action;
            }
        }

        public void Unregister(string name, UnityAction action)
        {
            if (eventDic.ContainsKey(name))
            {
                (eventDic[name] as EventInfo).actions -= action;
            }
        }

        public void Trigger<T>(string name, T info)
        {
            if (eventDic.ContainsKey(name))
            {
                if (eventDic[name] is not EventInfo<T>)
                {
                    Debug.LogError($"trigger is {typeof(EventInfo<T>)},but register is {eventDic[name].GetType()}");
                    return;
                }
                (eventDic[name] as EventInfo<T>).actions?.Invoke(info);
            }
        }

        public void Trigger<T1, T2>(string name, T1 Param1, T2 Param2)
        {
            if (eventDic.ContainsKey(name))
            {
                if ((eventDic[name] as EventInfo<T1, T2>) == null)
                {
                    Debug.LogError($"{eventDic[name].GetType()}回调，未对应事件类型{typeof(T1)},{typeof(T2)}");
                }
                (eventDic[name] as EventInfo<T1, T2>).actions?.Invoke(Param1, Param2);
            }
        }

        public void Trigger(string name)
        {
            if (eventDic.ContainsKey(name))
            {
                (eventDic[name] as EventInfo).actions?.Invoke();
            }
        }

        public void SpecificTrigger<T>(string name, T info, object obj)
        {
            if (eventDic.ContainsKey(name))
            {
                if (eventDic[name] is not EventInfo<T>)
                {
                    Debug.LogError($"trigger is {typeof(EventInfo<T>)},but register is {eventDic[name].GetType()}");
                    return;
                }
                if ((eventDic[name] as EventInfo<T>).objectEventDic.ContainsKey(obj))
                {
                    (eventDic[name] as EventInfo<T>).objectEventDic[obj]?.Invoke(info);
                }
            }
        }

        public void SpecificTrigger<T1, T2>(string name, T1 Param1, T2 Param2, object obj)
        {
            if (eventDic.ContainsKey(name))
            {
                if ((eventDic[name] as EventInfo<T1, T2>) == null)
                {
                    Debug.LogError($"{eventDic[name].GetType()}回调，未对应事件类型{typeof(T1)},{typeof(T2)}");
                }
                if ((eventDic[name] as EventInfo<T1, T2>).objectEventDic.ContainsKey(obj))
                {
                    (eventDic[name] as EventInfo<T1, T2>).objectEventDic[obj]?.Invoke(Param1, Param2);
                }
            }
        }

        public void SpecificTrigger(string name, object obj)
        {
            if (eventDic.ContainsKey(name))
            {
                if ((eventDic[name] as EventInfo).objectEventDic.ContainsKey(obj))
                {
                    (eventDic[name] as EventInfo).objectEventDic[obj]?.Invoke();
                }
            }
        }

        public void Clear()
        {
            eventDic.Clear();
        }

        public void EventUnregister(IListener caller)
        {
            foreach (var item in eventDic)
            {
                item.Value.RemoveListener(caller);
            }
        }

        public void Unregister(string eventName, IListener caller)
        {
            foreach (var item in eventDic)
            {
                if (item.Key == eventName)
                {
                    item.Value.RemoveListener(caller);
                }
            }
        }

        private EventCenter()
        {

        }
    }
}

