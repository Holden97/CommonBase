//使用utf-8
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace CommonBase
{
    public class Toggle : MonoBehaviour, IToggle, IPointerClickHandler
    {
        public UnityEvent OnSelect;
        public UnityEvent OnUnselect;
        public Action<IToggle> OnClickToggle;

        public string toggleTag;

        public bool isToggled;

        public bool IsToggle { get { return isToggled; } set { isToggled = value; } }

        public string ToggleTag => toggleTag;

        public Action<IToggle> OnClick { get => OnClickToggle; set => OnClickToggle = value; }

        public void OnPointerClick(PointerEventData eventData)
        {
            OnClick(this);
        }

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