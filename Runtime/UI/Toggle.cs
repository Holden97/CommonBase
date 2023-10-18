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
    public class Toggle : Button, IToggle
    {
        public UnityEvent OnToggleSelect;
        public UnityEvent OnToggleUnselect;
        public Action<IToggle> OnClickToggle;

        public string toggleTag;

        public bool isToggled;

        public bool IsToggle { get { return isToggled; } set { isToggled = value; } }

        public string ToggleTag => toggleTag;

        public Action<IToggle> OnClick { get => OnClickToggle; set => OnClickToggle = value; }

        public override void OnPointerClick(PointerEventData eventData)
        {
            base.OnPointerClick(eventData);
            OnClick(this);
        }

        public void OnSelected()
        {
            OnToggleSelect?.Invoke();
        }

        public void OnUnselected()
        {
            OnToggleUnselect?.Invoke();
        }
    }
}