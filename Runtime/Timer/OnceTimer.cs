using System;

namespace CommonBase
{
    /// <summary>
    /// 单次计时器 延迟x秒后，触发一个事件
    /// </summary>
    public class OnceTimer : BaseTimer
    {
        public OnceTimer(float interval, Action OnTrigger = null) : base(interval)
        {
            this.interval = interval;
            this.OnTrigger = OnTrigger;
        }
        protected override void OnDone()
        {
            this.OnTrigger?.Invoke();
            this.Dispose();
        }
    }
}
