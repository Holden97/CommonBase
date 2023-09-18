//使用utf-8
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace CommonBase
{
    public class ToggleGroup : MonoBehaviour, IToggleGroup
    {
        public List<IToggle> toggles;
        public void Trigger(IToggle toggle)
        {
            if (!toggle.IsToggle)
            {
                toggle.IsToggle = true;
                toggle.OnSelected();
                foreach (var t in toggles)
                {
                    if (t != toggle)
                    {
                        t.OnUnselected();
                        t.IsToggle = false;
                    }
                }
            }
        }

        public void Initialize()
        {
            toggles = SetToggles();
            SetActionToAllToggle((t) => Trigger(t));

            for (int i = 0; i < toggles.Count; i++)
            {
                IToggle t = toggles[i];
                if (i == 0)
                {
                    t.OnSelected();
                    t.IsToggle = true;
                }
                else
                {
                    t.OnUnselected();
                    t.IsToggle = false;
                }
            }
        }

        public virtual List<IToggle> SetToggles()
        {
            return GetComponentsInChildren<IToggle>().ToList();
        }

        public void SetActionToAllToggle(Action<IToggle> action)
        {
            foreach (var item in toggles)
            {
                if (item is MonoBehaviour m && m.TryGetComponent<Button>(out var b))
                {
                    b.onClick.AddListener(() => action(item));
                }
            }
        }
    }
}