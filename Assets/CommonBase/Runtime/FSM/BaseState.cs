using System.Collections.Generic;

namespace CommonBase
{
    public abstract class BaseState
    {
        protected int stateID;
        public int StateID { get => stateID; }
        public Dictionary<int, int> transitionDic;
        public FiniteStateMachine finiteStateMachine;

        /// <summary>
        /// 状态开始
        /// </summary>
        public virtual void OnStateStart()
        {

        }

        /// <summary>
        /// 状态更新
        /// </summary>
        public virtual void OnStateUpdate()
        {

        }

        /// <summary>
        /// 状态结束
        /// </summary>
        public virtual void OnStateEnd()
        {

        }

        /// <summary>
        /// 状态切换检测
        /// </summary>
        public virtual void OnStateCheckTransition()
        {

        }

        /// <summary>
        /// 状态机重置状态时[期望清除数据]
        /// </summary>
        public virtual void OnReset()
        {

        }

        public void AddTransition(int transition, int stateID)
        {
            if (transitionDic.ContainsKey(transition))
            {
                return;
            }
            transitionDic.Add(transition, stateID);
        }

        public void RemoveTransition(int transition)
        {
            if (transitionDic.ContainsKey(transition))
            {
                transitionDic.Remove(transition);
            }
        }

        public BaseState(int stateID)
        {
            this.transitionDic = new Dictionary<int, int>();
            this.stateID = stateID;
        }
    }

}
