//使用utf-8
using System;
using System.Collections.Generic;
using UnityEngine;

namespace CommonBase
{
    [CreateAssetMenu(fileName = "FSMSO", menuName = "ScriptableObjects/FSMSO", order = 1)]
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
        public string startState;
        public string trransition;
        public string endState;
    }

    [Serializable]
    public class FSMState
    {
        public string stateName;
        public string stateClass;
        public bool isDefaultState;
    }
}
