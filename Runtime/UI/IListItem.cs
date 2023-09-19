//使用utf-8
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace CommonBase
{
    public interface IListItem<T>
    {
        public T ItemInfo { get; }
        void BindData(T data);
    }

    public interface IToggle
    {
        public string ToggleTag { get; }
        Action<IToggle> OnClick { get; set; }
        bool IsToggle { get; set; }
        void OnUnselected();
        void OnSelected();
    }

    public interface IToggleGroup
    {
        void Trigger(IToggle toggle);
        void SetActionToAllToggle(Action<IToggle> action);
        void Initialize(string tag);
        List<IToggle> SetToggles(string tag);
    }
}


