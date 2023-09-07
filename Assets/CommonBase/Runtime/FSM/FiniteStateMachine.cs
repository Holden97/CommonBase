using System.Collections.Generic;

namespace CommonBase
{
    public class FiniteStateMachine
    {
        /// <summary>
        /// 当前状态
        /// </summary>
        public BaseState curState;
        /// <summary>
        /// 默认状态
        /// </summary>
        public BaseState defaultState;
        /// <summary>
        /// 状态字典
        /// </summary>
        private Dictionary<int, BaseState> statesDic;

        public void SetDefaultState(BaseState state)
        {
            defaultState = state;
        }

        public void SetDefaultState(int stateId)
        {
            defaultState = statesDic[stateId];
        }

        public void Start()
        {
            curState = defaultState;
            curState.OnStateStart();
        }

        public void Update()
        {
            curState.OnStateUpdate();
            curState.OnStateCheckTransition();
        }

        public void OnDestroy()
        {
            curState.OnStateEnd();
        }

        public FiniteStateMachine()
        {
            statesDic = new Dictionary<int, BaseState>();
        }

        /// <summary>
        /// 重置FSM状态[不清除数据]
        /// </summary>
        public void Reset()
        {
            foreach (var item in statesDic)
            {
                item.Value.OnReset();
            }
            curState = defaultState;
            curState.OnStateStart();
        }

        public void AddState(BaseState state)
        {
            if (!statesDic.ContainsKey(state.StateID))
            {
                statesDic.Add(state.StateID, state);
                state.finiteStateMachine = this;
            }
        }

        public void RemoveState(BaseState state)
        {
            if (statesDic.ContainsKey(state.StateID))
            {
                statesDic.Remove(state.StateID);
            }
        }

        public void Transform(int transition)
        {
            if (curState.transitionDic.ContainsKey(transition))
            {
                curState.OnStateEnd();

                var curStateID = curState.transitionDic[transition];
                curState = statesDic[curStateID];
                curState.OnStateStart();
            }
        }
    }
}
