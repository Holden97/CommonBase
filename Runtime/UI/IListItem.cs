//使用utf-8
using System;
using System.Collections.Generic;
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

        public Action<IToggle> OnClickToggle { get; set; }
        void OnToggleUnselect();
        void OnToggleSelect();

        public void AddSelectedAction(UnityAction<IToggle> action);
        public void AddUnselectedAction(UnityAction<IToggle> action);
    }

    public interface IToggleGroup
    {
        void SelectToggle(IToggle toggle);
        public void SetOnSelectedToAll(UnityAction<IToggle> action);
        public void SetOnUnselectedToAll(UnityAction<IToggle> action);

        void Initialize();
        List<IToggle> SetToggles(string tag);
    }
}


