//使用utf-8
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace CommonBase
{
    public interface IListItem
    {
        /// <summary>
        /// 只初始化一次
        /// </summary>
        /// <param name="data"></param>
        void Initialize(object data);
        /// <summary>
        /// 可以被绑定多次
        /// </summary>
        /// <param name="data"></param>
        void BindData(object data);
        bool InUse { get; set; }
        bool Initialized { get; set; }

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


