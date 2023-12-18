using System;

namespace CommonBase
{
    [Serializable]
    public class BaseState
    {
        public static string TRANSITION_REQ = "TRANSITION_REQ";
        public static string RESET_REQ = "RESET_REQ";

        public int stateID;
        public string stateName;
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

        public BaseState(string stateName)
        {
            this.stateName = stateName;
        }

        public void Transfer(string transition)
        {
            this.EventTrigger(TRANSITION_REQ, transition);
        }

        public void RequestReset()
        {
            this.EventTrigger(RESET_REQ);
        }
    }

}
