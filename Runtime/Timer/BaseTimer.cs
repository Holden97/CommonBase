using CommonBase;
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
        protected bool isExpired;

        public bool autoTick = true;

        public BaseTimer(float interval = 0)
        {
            this.id = TimerManager.timerSeed++;
            this.interval = interval;
            isCompleted = false;
            isExpired = true;
        }

        public void Start()
        {
            this._startTime = GetWorldTime();
            this._lastUpdateTime = GetWorldTime();
            this._nextTriggerTime = GetNextTriggerTime();
            isExpired = false;
        }

        public float CurProgress
        {
            get
            {
                if (_nextTriggerTime == _startTime)
                {
                    Debug.LogError("触发时间与开始时间相同，请检查！");
                    return 1;
                }
                return (_lastUpdateTime - _startTime) / (_nextTriggerTime - _startTime);
            }
        }

        public virtual void Tick()
        {
            if (isExpired || isCompleted) { return; }
            OnUpdate?.Invoke(GetWorldTime() - _lastUpdateTime);
            _lastUpdateTime = GetWorldTime();
            if (_lastUpdateTime > _nextTriggerTime)
            {
                //最后以完成执行一次update，为UI等元素争取一帧的时间
                OnUpdate?.Invoke(GetWorldTime() - _lastUpdateTime);
                OnDone();
            }
        }

        public void SetInterval(float interval)
        {
            this.interval = interval;
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

        public void Stop()
        {
            isExpired = true;
        }

        public void Resume()
        {
            isExpired = false;
        }


        protected float GetWorldTime()
        {
            return Time.time;
        }

        protected virtual float GetNextTriggerTime()
        {
            return this._startTime + this.interval;
        }

        public void Reset()
        {
            this._startTime = GetWorldTime();
            this._nextTriggerTime = GetNextTriggerTime();
            isExpired = false;
            isCompleted = false;
        }

    }
}
