//使用utf-8

using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Serialization;

namespace CommonBase
{
    [CreateAssetMenu(fileName = "FSMSO", menuName = "SO/FSMSO", order = 1)]
    [Serializable]
    public class FSMSO : ScriptableObject
    {
        public string assemblyName;
        public List<FSMState> states;
        public List<Transfer> transfers;
    }

    [Serializable]
    public class Transfer
    {
        [LabelText("开始状态")] 
        public string startState;
        [LabelText("转变条件")] 
        public string transition;
        [LabelText("结束状态")] 
        public string endState;
    }

    [Serializable]
    public class FSMState
    {
        [LabelText("状态名称")] 
        public string stateName;
        [LabelText("C#类名")] 
        public string stateClass;
        [LabelText("是否是默认状态")] 
        public bool isDefaultState;
    }
}