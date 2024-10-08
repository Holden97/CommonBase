using System;
using UnityEngine;

namespace CommonBase
{
    public abstract class BaseTimer
    {
        public int id;
        public int owner;

        public bool isCompleted;
        public float interval;
        public Action OnStart;
        public Action OnTrigger;
        private Action OnStop;
        private Action OnResume;
        public Action<float> OnUpdate;
        public bool triggerOnStart;

        protected float _startTime;
        protected float _lastUpdateTime;
        protected float _nextTriggerTime;
        protected bool isStopped;

        public bool autoTick = true;

        public BaseTimer(float interval = 0)
        {
            this.id = TimerManager.timerSeed++;
            this.interval = interval;
            isCompleted = false;
            isStopped = true;
        }

        /// <summary>
        /// 开始，如果已经开始，则不重新开始
        /// </summary>
        public virtual void Start()
        {
            if (isStopped || isCompleted)
            {
                Reset();

                isStopped = false;
                isCompleted = false;
            }
            this.OnStart?.Invoke();
            if (this.triggerOnStart)
            {
                this.OnTrigger?.Invoke();
            }
        }

        /// <summary>
        /// 强制重置
        /// </summary>
        public virtual void Reset()
        {
            this._startTime = GetWorldTime();
            this._lastUpdateTime = GetWorldTime();
            this._nextTriggerTime = GetNextTriggerTime();
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
            if (isStopped || isCompleted) { return; }
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

        public void AddInterval(float offset)
        {
            this.interval += offset;
            this._nextTriggerTime = GetNextTriggerTime();
        }

        protected virtual void OnDone()
        {
        }

        /// <summary>
        /// 结束，随时可以丢弃
        /// </summary>
        public virtual void Stop()
        {
            isStopped = true;
            isCompleted = true;
            OnStop?.Invoke();
        }

        /// <summary>
        /// 暂停，保持引用，不能随意丢弃
        /// </summary>
        public virtual void Paused()
        {
            isStopped = true;
            OnStop?.Invoke();
        }

        public virtual void Resume()
        {
            isStopped = false;
            OnResume?.Invoke();
        }


        protected float GetWorldTime()
        {
            return Time.time;
        }

        protected virtual float GetNextTriggerTime()
        {
            return this._startTime + this.interval;
        }
    }
}
