//使用utf-8
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace CommonBase
{
    public interface ISelectionListItem : IListItem
    {
        void OnSelected();

        void ToSelected(float duration);
        void ToUnselected(float duration);
        void OnUnselected();
        public bool IsSelected { get; set; }
    }

}


