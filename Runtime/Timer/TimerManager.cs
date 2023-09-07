using System.Collections.Generic;

namespace CommonBase
{
    public class TimerManager : Singleton<TimerManager>
    {
        public Dictionary<int, List<Timer>> timerDic;
        private List<Timer> addTimerList = new List<Timer>();
        private List<Timer> removeTimerList = new List<Timer>();

        private TimerManager()
        {
        }

        public override void OnCreateInstance()
        {
            base.OnCreateInstance();
            timerDic = new Dictionary<int, List<Timer>>();
        }

        private int GetTimersCout(int instanceID)
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

        public override void Tick()
        {

            //foreach在MoveNext还在执行的过程中不能够修改每个item中的值，比如这里就不能修改每个timerList中的值
            base.Tick();
            foreach (var timerList in timerDic)
            {
                foreach (var item in timerList.Value)
                {
                    item.Tick();
                }
                timerList.Value.RemoveAll(t => t.isDone && !t.isLoop);
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

        public void RegisterTimer(Timer timer)
        {
            addTimerList.Add(timer);
        }

        /// <summary>
        /// 添加计时器，并执行开始的回调
        /// </summary>
        /// <param name="timer"></param>
        private void Add(Timer timer)
        {
            if (timerDic.TryGetValue(timer.owner, out var timers))
            {
                if (!timer.singleton || (timer.singleton && timers.Find(x => x.timerName == timer.timerName) == null))
                {
                    //周期大于0的计时器才放入列表中，否则就执行一次
                    OnStart(timer);
                    if (timer.period > 0)
                    {
                        timers.Add(timer);
                    }
                    else
                    {
                        timer.OnComplete?.Invoke();
                    }
                }
            }
            else
            {
                if (timer.period > 0)
                {
                    this.timerDic.Add(timer.owner, new List<Timer>() { timer });
                }
                OnStart(timer);
            }
        }

        private static void OnStart(Timer timer)
        {
            timer.OnStart?.Invoke();
            if (timer.triggerOnStart)
            {
                timer.OnComplete?.Invoke();
            }
        }

        public void UnregisterTimer(Timer timer)
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

        public void UnregisterTimer(int instanceID, string timerName)
        {
            if (timerDic.TryGetValue(instanceID, out var timers))
            {
                var curTimer = timers.Find(t => t.timerName == timerName);
                if (curTimer != null)
                {
                    timers.Remove(curTimer);
                }
            }
        }

        public void UnregisterTimer(string timerName)
        {
            foreach (var owenrList in timerDic.Values)
            {
                var curTimer = owenrList.Find(t => t.timerName == timerName);
                if (curTimer != null)
                {
                    owenrList.Remove(curTimer);
                }
            }
        }

        public void UnregisterTimer(int instanceID, ETimerType timerType)
        {
            if (timerDic.TryGetValue(instanceID, out var timers))
            {
                foreach (var item in timers)
                {
                    if (item.timerType == timerType)
                    {
                        timers.Remove(item);
                    }
                }
            }
        }
    }
}
