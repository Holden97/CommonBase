using System;
using UnityEngine;

namespace CommonBase
{
    /// <summary>
    /// 计时器类，不支持间隔为0的循环计时器
    /// 循环计时器 每隔x秒，触发一次事件
    /// </summary>
    public class RandomLoopTimer : BaseTimer
    {
        Func<float> getNextInterval;

        public RandomLoopTimer(Func<float> getNextInterval, Action OnStart = null, Action onTrigger = null,
            int ownerId = -1, bool triggerOnStart = false) : base()
        {
            this.owner = ownerId;
            this.interval = getNextInterval();
            this.OnStart = OnStart;
            this.triggerOnStart = triggerOnStart;
            this.OnTrigger = onTrigger;
            this.getNextInterval = getNextInterval;
            this._nextTriggerTime = GetNextTriggerTime();
        }

        protected override void OnDone()
        {
            this.OnTrigger?.Invoke();
            this._startTime = GetWorldTime();
            this._nextTriggerTime = GetNextTriggerTime();
        }

        protected override float GetNextTriggerTime()
        {
            if (getNextInterval != null)
            {
                this.interval = getNextInterval();
            }
            return _startTime + this.interval;
        }
    }

}
