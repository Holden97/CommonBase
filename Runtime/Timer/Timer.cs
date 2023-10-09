using System;
using UnityEngine;

namespace CommonBase
{
    /// <summary>
    /// 计时器类，不支持间隔为0的循环计时器
    /// </summary>
    public class Timer
    {
        public static int seed = 0;
        public int id;
        public int owner;
        public float period;
        public bool isDone;
        public Action OnComplete;
        public Action<float> OnUpdate;
        public Action OnStart;
        public string timerName;
        public bool isLoop;
        /// <summary>
        /// 若为true,同时只能注册一个相同名称的计时器
        /// </summary>
        public bool singleton;
        public bool triggerOnStart;

        private float _startTime;
        private float _lastUpdateTime;
        private float _endTime;
        private bool _isPause;

        /// <summary>
        /// 主线程构造
        /// </summary>
        /// <param name="period">持续时间，单位:s</param>
        /// <param name="onComplete"></param>
        /// <param name="onUpdate"></param>
        /// <param name="ownerId"></param>
        public Timer(float period, string timerName = null, Action OnStart = null, Action onComplete = null, Action<float> onUpdate = null, ETimerType timerType = ETimerType.trigger, int ownerId = -1, bool isLoop = false, bool triggerOnStart = false, bool singleton = false)
        {
            this.id = seed++;
            this.owner = ownerId;
            this.period = period;
            OnComplete = onComplete;
            OnUpdate = onUpdate;
            this.OnStart = OnStart;
            this.triggerOnStart = triggerOnStart;

            _startTime = GetWorldTime();
            _lastUpdateTime = GetWorldTime();
            this._endTime = GetDoneTime();

            isDone = false;
            if (timerName != null)
            {
                this.timerName = timerName;
            }
            else
            {
                this.timerName = this.id.ToString();
            }
            this.isLoop = isLoop;
            this.singleton = singleton;
            if (singleton && timerName == null)
            {
                Debug.LogError("你指定了一个单例计时器，但未指定它的名称，确定吗？");
            }
        }

        public void SetDuration(float duration)
        {
            this.period = duration;
            this._endTime = GetDoneTime();
        }

        public void Dispose()
        {

        }

        internal void OnDone()
        {
            OnComplete?.Invoke();
            if (isLoop)
            {
                _startTime = GetWorldTime();
                this._endTime = GetDoneTime();
            }
            else
            {
                isDone = true;
            }
        }

        public void Pause()
        {
            _isPause = true;
        }

        public void Resume()
        {
            _isPause = false;
        }

        internal void Tick()
        {
            if (_isPause) { return; }
            OnUpdate?.Invoke(GetWorldTime() - _lastUpdateTime);
            _lastUpdateTime = GetWorldTime();
            if (_lastUpdateTime > _endTime)
            {
                OnDone();
            }
        }

        private float GetWorldTime()
        {
            //return this.usesRealTime ? Time.realtimeSinceStartup : Time.time;
            return Time.time;
        }

        private float GetDoneTime()
        {
            return this._startTime + this.period;
        }
    }

    public enum ETimerType
    {
        /// <summary>
        /// 持续性计时，如果立刻要去做新的工作，则该计时器会被打断。
        /// </summary>
        Continous,
        /// <summary>
        /// 触发式计时，如果立刻要去做新的工作，该计时器继续计时，不会被打断。
        /// </summary>
        trigger,
    }
}
