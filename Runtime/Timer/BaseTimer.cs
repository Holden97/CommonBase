using System;
using UnityEngine;

namespace CommonBase
{
    public abstract class BaseTimer
    {
        public int id;
        public int owner;

        public float totalTime;
        public float pastTime;

        public bool isCompleted;
        public float interval;
        public Action OnStart;
        public Action OnTrigger;
        public Action OnDispose;
        public Action<float> OnUpdate;
        public bool triggerOnStart;

        protected float _startTime;
        protected float _lastUpdateTime;
        protected float _nextTriggerTime;
        protected bool _isPause;

        public BaseTimer(float interval)
        {
            this.id = TimerManager.timerSeed++;

            isCompleted = false;
            this.interval = interval;
            _startTime = GetWorldTime();
            _lastUpdateTime = GetWorldTime();
            this._nextTriggerTime = GetNextTriggerTime();
        }

        public float CurProgress => (_lastUpdateTime - _startTime) / (_nextTriggerTime - _startTime);

        public virtual void Tick()
        {
            if (_isPause || isCompleted) { return; }
            OnUpdate?.Invoke(GetWorldTime() - _lastUpdateTime);
            _lastUpdateTime = GetWorldTime();
            if (_lastUpdateTime > _nextTriggerTime)
            {
                //最后以完成执行一次update，为UI等元素争取一帧的时间
                OnUpdate?.Invoke(GetWorldTime() - _lastUpdateTime);
                OnDone();
            }
        }

        public void SetDuration(float duration)
        {
            this.interval = duration;
            this._nextTriggerTime = GetNextTriggerTime();
        }

        public void Dispose()
        {
            isCompleted = true;
            OnDispose?.Invoke();
        }

        protected virtual void OnDone()
        {
        }

        public void Pause()
        {
            _isPause = true;
        }

        public void Resume()
        {
            _isPause = false;
        }


        protected float GetWorldTime()
        {
            return Time.time;
        }

        protected float GetNextTriggerTime()
        {
            return this._startTime + this.interval;
        }

    }
}
