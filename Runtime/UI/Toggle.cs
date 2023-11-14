//使用utf-8
using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace CommonBase
{
    public class Toggle : Button, IToggle
    {
        protected Action OnSelected;
        protected Action OnUnselected;
        [SerializeField]
        private UnityEvent unselectEvent;
        [SerializeField]
        private UnityEvent selectEvent;
        private Action<IToggle> onClickToggle;

        public string toggleTag;
        private string id;

        public bool isToggled;

        public bool IsToggle { get { return isToggled; } set { isToggled = value; } }
        public string ID { get => id; set { id = value; } }

        public string ToggleTag => toggleTag;

        Action<IToggle> IToggle.OnClickToggle { get => onClickToggle; set => onClickToggle = value; }

        public void AddSelectedAction(UnityAction<IToggle> action)
        {
            selectEvent.AddListener(() => action(this));
        }


        public void AddUnselectedAction(UnityAction<IToggle> action)
        {
            unselectEvent.AddListener(() => action(this));
        }

        public void OnToggleSelect()
        {
            OnSelected?.Invoke();
            selectEvent?.Invoke();
        }

        public void OnToggleUnselect()
        {
            OnUnselected?.Invoke();
            unselectEvent?.Invoke();
        }

        public override void OnPointerClick(PointerEventData eventData)
        {
            base.OnPointerClick(eventData);
            this.onClickToggle(this);
        }
    }
}