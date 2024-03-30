using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CommonBase
{
    public static class TimerExtension
    {
        public static T Register<T>(this T timer) where T : BaseTimer
        {
            TimerManager.Instance.RegisterTimer(timer);
            return timer;
        }

        public static T Unregister<T>(this T timer) where T : BaseTimer
        {
            TimerManager.Instance.UnregisterTimer(timer);
            return timer;
        }
    }
}
