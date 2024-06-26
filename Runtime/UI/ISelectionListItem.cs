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
        void OnUnselected();
        public bool IsSelected { get; set; }
    }

}


