using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace CommonBase
{
    [Serializable]
    public class FiniteStateMachine : IListener
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
        public List<BaseState> statesDic;
        public FSMSO fsmData;


        public FSMSO FSMData { get => fsmData; private set { fsmData = value; } }

        public BaseState GetState(string key)
        {
            return statesDic.Find(x => x.stateName == key);
        }

        public FiniteStateMachine(FSMSO fsmData)
        {
            statesDic = new List<BaseState>();
            this.fsmData = fsmData;
            var assembly = Assembly.Load(fsmData.assemblyName);
            foreach (var item in fsmData.states)
            {
                var type = assembly.GetType(item.stateClass);
                ConstructorInfo constructor = type.GetConstructor(new[] { typeof(string) });

                if (constructor != null)
                {
                    curState = (BaseState)constructor.Invoke(new object[] { item.stateName });
                    AddState(curState);
                    if (item.isDefaultState)
                    {
                        SetDefaultState(curState);
                    }
                }

            }
        }

        public void SetDefaultState(BaseState state)
        {
            defaultState = state;
        }

        public void SetDefaultState(string stateName)
        {
            defaultState = GetState(stateName);
        }

        public void Start()
        {
            this.EventRegister<string>(BaseState.TRANSITION_REQ, OnTransistionReq);
            this.EventRegister(BaseState.RESET_REQ, OnResetReq);
            curState = defaultState;
            curState.OnStateStart();
        }

        public void Stop()
        {
            this.EventUnregisterAll();
        }

        private void OnResetReq()
        {
            this.Reset();
        }

        private void OnTransistionReq(string arg0)
        {
            this.Transfer(arg0);
        }

        private void OnStateCheck(string transferName)
        {
        }

        public void Update()
        {
            curState.OnStateUpdate();
            curState.OnStateCheckTransition();
        }

        public void OnDestroy()
        {
            this.curState.OnDispose();
            this.EventUnregisterAll();
        }

        /// <summary>
        /// 重置FSM状态[不清除数据]
        /// </summary>
        public void Reset()
        {
            foreach (var item in statesDic)
            {
                item.OnReset();
            }
            curState = defaultState;
            curState.OnStateStart();
        }

        public void AddState(BaseState state)
        {
            if (!statesDic.Exists(x => x.stateName == state.stateName))
            {
                statesDic.Add(state);
            }
        }

        public void Transfer(string transition)
        {
            foreach (var transfer in this.FSMData.transfers)
            {
                if (transfer.transition == transition && transfer.startState == this.curState.stateName)
                {
                    this.curState.OnStateEnd();
                    Debug.Log("FSM change state from " + curState.stateName + " to " + this.GetState(transfer.endState).stateName);
                    this.curState = this.GetState(transfer.endState);
                    this.curState.OnStateStart();
                }
            }
        }

        public void RemoveState(BaseState state)
        {
            var curState = statesDic.Find(x => x.stateName == state.stateName);
            if (curState != null)
            {
                statesDic.Remove(curState);
            }
        }
    }
}
