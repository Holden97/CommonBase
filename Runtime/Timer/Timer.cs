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

        public Action<float> OnUpdate;
        public Action OnStart;
        public string timerName;
        public bool isLoop;
        /// <summary>
        /// 断言式，为真时表示当前计时器满足继续的条件。
        /// </summary>
        public Func<bool> assertion;
        public float Progress => (_lastUpdateTime - _startTime) / (_endTime - _startTime);
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
        /// 计时器单次计时结束
        /// </summary>
        public Action OnComplete;
        /// <summary>
        /// 计时器最终完成
        /// </summary>
        private Action OnFinish;

        /// <summary>
        /// 构造
        /// </summary>
        /// <param name="period">持续时间，单位/s</param>
        /// <param name="timerName"></param>
        /// <param name="OnStart"></param>
        /// <param name="onComplete"></param>
        /// <param name="onUpdate"></param>
        /// <param name="timerType"></param>
        /// <param name="ownerId"></param>
        /// <param name="isLoop"></param>
        /// <param name="triggerOnStart"></param>
        /// <param name="singleton"></param>
        /// <param name="assertion">断言，为真时持续执行</param>
        public Timer(float period, string timerName = null, Action OnStart = null, Action onComplete = null, Action<float> onUpdate = null, ETimerType timerType = ETimerType.trigger, int ownerId = -1, bool isLoop = false, bool triggerOnStart = false, bool singleton = false, Func<bool> assertion = null, Action OnFinish = null)
        {
            this.id = seed++;
            this.owner = ownerId;
            this.period = period;
            OnComplete = onComplete;
            AddUpdate(onUpdate);
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
            this.assertion = assertion;
            if (singleton && timerName == null)
            {
                Debug.LogError("你指定了一个单例计时器，但未指定它的名称，确定吗？");
            }
            this.OnFinish = OnFinish;
        }

        public void AddUpdate(Action<float> onUpdate)
        {
            OnUpdate = onUpdate;
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
                OnFinish?.Invoke();
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
            if (assertion != null && !assertion.Invoke())
            {
                isDone = true;
                OnFinish?.Invoke();
            }
            if (_isPause || isDone) { return; }
            OnUpdate?.Invoke(GetWorldTime() - _lastUpdateTime);
            _lastUpdateTime = GetWorldTime();
            if (_lastUpdateTime > _endTime)
            {
                //最后以完成执行一次update，为UI等元素争取一帧的时间
                OnUpdate?.Invoke(GetWorldTime() - _lastUpdateTime);
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
