using System;
using UnityEngine;

namespace CommonBase
{
    /// <summary>
    /// 有限时长计时器，在有限时间Y内，计时器始终每隔X秒，触发一次事件
    /// </summary>
    public class ScheduledTimer : BaseTimer
    {
        public static int seed = 0;
        public bool isDone;
        public bool isLoop;
        /// <summary>
        /// 断言式，为真时表示当前计时器满足继续的条件。
        /// </summary>
        public Func<bool> assertion;
        public float Progress => (_lastUpdateTime - _startTime) / (_nextTriggerTime - _startTime);

        /// <summary>
        /// 若为true,同时只能注册一个相同名称的计时器
        /// </summary>
        public bool singleton;

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
        public ScheduledTimer(float period, string timerName = null, Action OnStart = null, Action onComplete = null, Action<float> onUpdate = null, int ownerId = -1, bool isLoop = false, bool triggerOnStart = false, bool singleton = false, Func<bool> assertion = null, Action OnFinish = null) : base()
        {
            this.id = seed++;
            this.interval = period;
            this.owner = ownerId;
            OnTrigger = onComplete;
            AddUpdate(onUpdate);
            this.OnStart = OnStart;
            this.triggerOnStart = triggerOnStart;

            _startTime = GetWorldTime();
            _lastUpdateTime = GetWorldTime();

            isDone = false;
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


        public override void Tick()
        {
            if (assertion != null && !assertion.Invoke())
            {
                isDone = true;
                OnComplete?.Invoke();
            }
            if (_isPause || isDone) { return; }
            OnUpdate?.Invoke(GetWorldTime() - _lastUpdateTime);
            _lastUpdateTime = GetWorldTime();
            if (_lastUpdateTime > _nextTriggerTime)
            {
                //最后以完成执行一次update，为UI等元素争取一帧的时间
                OnUpdate?.Invoke(GetWorldTime() - _lastUpdateTime);
                OnDone();
            }
        }


        private float GetDoneTime()
        {
            return this._startTime + this.interval;
        }

    }

}
