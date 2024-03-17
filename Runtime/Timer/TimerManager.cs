using System.Collections.Generic;
using System.Linq;

namespace CommonBase
{
    public class TimerManager : MonoSingleton<TimerManager>
    {
        public static int timerSeed = 0;
        public Dictionary<int, List<BaseTimer>> timerDic;
        private List<BaseTimer> addTimerList;
        private List<BaseTimer> removeTimerList;

        private TimerManager()
        {
        }

        public override void Initialize()
        {
            base.Initialize();
            timerDic = new Dictionary<int, List<BaseTimer>>();
            addTimerList = new List<BaseTimer>();
            removeTimerList = new List<BaseTimer>();
        }

        private int GetTimersCount(int instanceID)
        {
            var curOwner = timerDic.TryGetValue(instanceID, out var timers);
            if (timers == null) { return 0; }
            return timers.Count;
        }

        private int GetAllTimersCout()
        {
            var result = 0;
            foreach (var item in timerDic)
            {
                result += item.Value.Count;
            }
            return result;
        }

        private void Update()
        {
            //foreach在MoveNext还在执行的过程中不能够修改每个item中的值，比如这里就不能修改每个timerList中的值
            foreach (var timerList in timerDic.ToList())
            {
                foreach (var item in timerList.Value)
                {
                    item.Tick();
                }
                timerList.Value.RemoveAll(t => t.isCompleted);
            }
            //删除旧的计时器
            for (int i = 0; i < removeTimerList.Count; i++)
            {
                if (timerDic.TryGetValue(removeTimerList[i].owner, out var timers))
                {
                    if (timers.Contains(removeTimerList[i]))
                    {
                        timers.Remove(removeTimerList[i]);
                    }
                }
            }
            removeTimerList?.Clear();
            //添加新的计时器
            for (int i = 0; i < addTimerList.Count; i++)
            {
                var timer = addTimerList[i];
                Add(timer);
            }
            addTimerList?.Clear();
            //Debug.Log($"正在计时的计时器个数:{GetAllTimersCout()}");
        }

        public void RegisterTimer(BaseTimer timer)
        {
            addTimerList.Add(timer);
        }

        /// <summary>
        /// 添加计时器，并执行开始的回调
        /// </summary>
        /// <param name="curTimer"></param>
        private void Add(BaseTimer timer)
        {
            TryAddAsLoopTimer(timer);
            TryAddAsOnceTimer(timer);
        }

        private void TryAddAsLoopTimer(BaseTimer timer)
        {
            if (timer is LoopTimer curTimer)
            {
                if (timerDic.TryGetValue(curTimer.owner, out var timers))
                {
                    //周期大于0的计时器才放入列表中，否则就执行一次
                    Launch(curTimer);
                    if (curTimer.interval > 0)
                    {
                        timers.Add(curTimer);
                    }
                    else
                    {
                        curTimer.OnTrigger?.Invoke();
                    }
                }
                else
                {
                    if (curTimer.interval > 0)
                    {
                        this.timerDic.Add(curTimer.owner, new List<BaseTimer>() { curTimer });
                    }
                    Launch(curTimer);
                }
            }
        }

        private void TryAddAsOnceTimer(BaseTimer timer)
        {
            if (timer is OnceTimer onceTimer)
            {
                if (timerDic.TryGetValue(onceTimer.owner, out var timers))
                {
                    //周期大于0的计时器才放入列表中，否则就执行一次
                    Launch(onceTimer);
                    if (onceTimer.interval > 0)
                    {
                        timers.Add(onceTimer);
                    }
                    else
                    {
                        onceTimer.OnTrigger?.Invoke();
                    }
                }
                else
                {
                    if (onceTimer.interval > 0)
                    {
                        this.timerDic.Add(onceTimer.owner, new List<BaseTimer>() { onceTimer });
                    }
                    Launch(onceTimer);
                }
            }
        }

        public static void Launch(BaseTimer timer)
        {
            timer.OnStart?.Invoke();
            if (timer.triggerOnStart)
            {
                timer.OnTrigger?.Invoke();
            }
        }

        public void UnregisterTimer(BaseTimer timer)
        {
            if (timer == null) { return; }
            if (timerDic.TryGetValue(timer.owner, out var timers))
            {
                if (timers.Contains(timer))
                {
                    removeTimerList.Add(timer);
                }
            }
        }
    }
}
