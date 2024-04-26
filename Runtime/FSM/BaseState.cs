using System;

namespace CommonBase
{
    [Serializable]
    public class BaseState
    {
        public static string TRANSITION_REQ = "TRANSITION_REQ";

        public int stateID;
        public string stateName;
        private FiniteStateMachine fsm;
        public int StateID { get => stateID; }

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

        public virtual void OnDispose()
        {

        }

        public BaseState(string stateName, FiniteStateMachine fsm)
        {
            this.stateName = stateName;
            this.fsm = fsm;
        }

        //TODO:不够直白，能不能直接转换成指定的状态？
        public void Transfer(string transition)
        {
            this.EventTrigger(TRANSITION_REQ, transition);
        }
    }

}
