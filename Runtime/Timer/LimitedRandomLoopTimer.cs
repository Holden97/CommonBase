using System;
using UnityEngine;

namespace CommonBase
{
    /// <summary>
    /// 计时器类，不支持间隔为0的循环计时器
    /// 有限随机计时器 每隔x秒，触发一次事件，直到Y秒后结束
    /// </summary>
    public class LimitedRandomLoopTimer : BaseTimer
    {
        Func<float> getNextInterval;
        protected float duration;
        protected float curTime;

        public LimitedRandomLoopTimer(Func<float> getNextInterval, float duration, Action OnStart = null, Action onTrigger = null,
            int ownerId = -1, bool triggerOnStart = false) : base()
        {
            this.owner = ownerId;
            this.interval = getNextInterval();
            this.OnStart = OnStart;
            this.triggerOnStart = triggerOnStart;
            this.OnTrigger = onTrigger;
            this.getNextInterval = getNextInterval;
            this._nextTriggerTime = GetNextTriggerTime();
            this.duration = duration;
            this.curTime = 0;
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

        public override void Tick()
        {
            if (isStopped || isCompleted) { return; }
            var curtime = GetWorldTime();
            var delta = GetWorldTime() - _lastUpdateTime;
            curTime += delta;
            OnUpdate?.Invoke(delta);
            _lastUpdateTime = curtime;
            if (_lastUpdateTime > _nextTriggerTime)
            {
                //最后以完成执行一次update，为UI等元素争取一帧的时间
                OnUpdate?.Invoke(GetWorldTime() - _lastUpdateTime);
                OnDone();
            }
            if (duration <= curTime)
            {
                this.Stop();
            }
        }
    }

}
