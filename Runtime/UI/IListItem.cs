//使用utf-8
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace CommonBase
{
    public interface IListItem
    {
        void BindData(object data);
    }

    public interface IToggle
    {
        public string ToggleTag { get; }
        public string ID { get; }
        bool IsToggle { get; set; }

        void OnToggleUnselect();
        void OnToggleSelect();

    }

    public interface IToggleGroup
    {
        public IToggle CurSelectedToggle { get; set; }
        void SelectToggle(IToggle toggle);
        void Initialize();
        List<IToggle> SetToggles(string tag);
    }
}


