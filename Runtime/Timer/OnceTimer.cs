using System;

namespace CommonBase
{
    /// <summary>
    /// 单次计时器 延迟x秒后，触发一个事件
    /// </summary>
    public class OnceTimer : BaseTimer
    {
        public OnceTimer(float duration, Action OnTrigger = null) : base(duration)
        {
            this.interval = duration;
            this.OnTrigger = OnTrigger;
        }
        protected override void OnDone()
        {
            this.OnTrigger?.Invoke();
            this.Dispose();
        }
    }
}
