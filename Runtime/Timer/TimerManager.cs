﻿using System.Collections.Generic;
using System.Linq;

namespace CommonBase
{
    public class TimerManager : MonoSingleton<TimerManager>
    {
        public static int timerSeed = 0;
        public Dictionary<int, List<BaseTimer>> timerDic;
        private List<BaseTimer> addTimerList;
        private List<BaseTimer> addTimerTempList;
        private List<BaseTimer> removeTimerList;
        private List<BaseTimer> removeTimerTempList;

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
            if (timerDic == null) return;
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
            removeTimerTempList = removeTimerList.ToList();
            foreach (BaseTimer bt in removeTimerTempList)
            {
                if (timerDic.TryGetValue(bt.owner, out var timers))
                {
                    if (timers.Contains(bt))
                    {
                        timers.Remove(bt);

                    }
                }
            }
            removeTimerList?.RemoveAll(x => removeTimerTempList.Contains(x));
            //添加新的计时器
            addTimerTempList = addTimerList.ToList();
            foreach (var timer in addTimerTempList)
            {
                TryAdd(timer);
            }
            addTimerList?.RemoveAll(x => addTimerTempList.Contains(x));
            //Debug.Log($"正在计时的计时器个数:{GetAllTimersCout()}");
        }

        public void RegisterTimer(BaseTimer timer)
        {
            timer.Start();
            addTimerList.Add(timer);
        }

        /// <summary>
        /// 添加计时器，并执行开始的回调
        /// </summary>
        /// <param name="curTimer"></param>

        private void TryAdd(BaseTimer timer)
        {
            if (timerDic.TryGetValue(timer.owner, out var timers))
            {
                //已有，那么不执行
                if (timers.Contains(timer)) return;
                //周期大于0的计时器才放入列表中，否则就执行一次
                if (timer.interval > 0)
                {
                    timers.Add(timer);
                }
            }
            else
            {
                if (timer.interval > 0)
                {
                    this.timerDic.Add(timer.owner, new List<BaseTimer>() { timer });
                }
            }
        }

        public void UnregisterTimer(BaseTimer timer)
        {
            if (timer == null) { return; }
            timer.Stop();
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
