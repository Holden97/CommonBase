using System;
using UnityEngine;

namespace CommonBase
{
    /// <summary>
    /// 计时器类，不支持间隔为0的循环计时器
    /// 1.单次计时器 延迟x秒后，触发一个事件
    /// 2.循环计时器 每隔x秒，触发一次事件
    /// 3.断言计时器，当断言条件为真时，计时器始终每隔X秒，触发一次事件
    /// 4.有限时长计时器，在有限时间Y内，计时器始终每隔X秒，触发一次事件
    /// </summary>
    public class BaseTimer : ITimer
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

        public string Name { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public bool IsDone { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public int Owner { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public float TotalTime { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public float pastTime { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

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
        /// 计时器触发
        /// </summary>
        public Action OnTrigger;
        /// <summary>
        /// 计时器最终完成
        /// </summary>
        private Action OnComplete;

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
        public BaseTimer(float period, string timerName = null, Action OnStart = null, Action onComplete = null, Action<float> onUpdate = null, int ownerId = -1, bool isLoop = false, bool triggerOnStart = false, bool singleton = false, Func<bool> assertion = null, Action OnFinish = null)
        {
            this.id = seed++;
            this.owner = ownerId;
            this.period = period;
            OnTrigger = onComplete;
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
            this.OnComplete = OnFinish;
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
            OnTrigger?.Invoke();
            if (isLoop)
            {
                _startTime = GetWorldTime();
                this._endTime = GetDoneTime();
            }
            else
            {
                isDone = true;
                OnComplete?.Invoke();
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
                OnComplete?.Invoke();
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

        void ITimer.Tick()
        {
            throw new NotImplementedException();
        }
    }

    public class SimpleTimer : ITimer
    {
        public float duration;
        public Action OnComplete;

        public string Name { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public bool IsDone { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public int Owner { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public float TotalTime { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public float pastTime { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public void Tick()
        {
            throw new NotImplementedException();
        }
    }

    public interface ITimer
    {
        float TotalTime { get; set; }
        float pastTime { get; set; }
        string Name { get; set; }
        bool IsDone { get; set; }
        int Owner { get; set; }

        void Tick();
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
