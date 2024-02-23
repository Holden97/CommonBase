using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CommonBase
{
    public static class TimerExtension
    {
        public static void Register(this LoopTimer timer)
        {
            TimerManager.Instance.RegisterTimer(timer);
        }

        public static void Unregister(this LoopTimer timer)
        {
            TimerManager.Instance.UnregisterTimer(timer);
        }
    }
}
