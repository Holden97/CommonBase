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
        public int totalLoopCount;
        public int curLoopCount;
        public LoopTimer(float interval, Action OnStart = null, Action onTrigger = null,
            int ownerId = -1, bool triggerOnStart = false, int loopCount = -1) : base(interval)
        {
            this.owner = ownerId;
            this.OnStart = OnStart;
            this.triggerOnStart = triggerOnStart;
            this.OnTrigger = onTrigger;
            this.totalLoopCount = loopCount;
        }

        public override void Start()
        {
            curLoopCount = 0;
            base.Start();
        }

        protected override void OnDone()
        {
            this.OnTrigger?.Invoke();
            this._startTime = GetWorldTime();
            this._nextTriggerTime = GetNextTriggerTime();
            this.curLoopCount++;
            if (this.totalLoopCount <= curLoopCount && totalLoopCount != -1)
            {
                this.isCompleted = true;
            }
        }
    }

}
