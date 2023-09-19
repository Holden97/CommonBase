//使用utf-8
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace CommonBase
{
    public class Toggle : MonoBehaviour, IToggle
    {
        public UnityEvent OnSelect;
        public UnityEvent OnUnselect;

        public bool isToggle;

        public bool IsToggle { get { return isToggle; } set { isToggle = value; } }

        public void OnSelected()
        {
            OnSelect?.Invoke();
        }

        public void OnUnselected()
        {
            OnUnselect?.Invoke();
        }
    }
}