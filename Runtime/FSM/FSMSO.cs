//使用utf-8
using System;
using System.Collections.Generic;
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
        public string startState;
        [FormerlySerializedAs("trransition")]
        public string transition;
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
