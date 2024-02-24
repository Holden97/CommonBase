using System;
using UnityEngine;

namespace CommonBase
{
    /// <summary>
    /// 计时器类，不支持间隔为0的循环计时器
    /// 循环计时器 每隔x秒，触发一次事件
    /// </summary>
    public class LoopTimer : BaseTimer
    {
        public LoopTimer(float interval, Action OnStart = null, Action onTrigger = null,
            int ownerId = -1, bool triggerOnStart = false) : base(interval)
        {
            this.interval = interval;
            this.owner = ownerId;
            this.OnStart = OnStart;
            this.triggerOnStart = triggerOnStart;
            this.OnTrigger = onTrigger;
        }

        protected override void OnDone()
        {
            this.OnTrigger?.Invoke();
            this._startTime = GetWorldTime();
            this._nextTriggerTime = GetNextTriggerTime();
        }
    }

}
