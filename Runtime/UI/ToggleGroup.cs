//使用utf-8
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
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

        public void Initialize(string tag = null)
        {
            toggles = SetToggles(tag);
            SetActionToAllToggle((t) => Trigger(t));
        }

        public void Apply()
        {
            for (int i = 0; i < toggles.Count; i++)
            {
                IToggle t = toggles[i];
                if (i == 0)
                {
                    t.OnSelected();
                    t.OnClick(t);
                    t.IsToggle = true;
                }
                else
                {
                    t.OnUnselected();
                    t.IsToggle = false;
                }
            }
        }

        public virtual List<IToggle> SetToggles(string tag = null)
        {
            if (tag == null)
            {
                return GetComponentsInChildren<IToggle>().ToList();
            }
            else
            {
                return GetComponentsInChildren<IToggle>().ToList().FindAll(x => x.ToggleTag == tag);
            }
        }

        public List<IToggle> GetToggles(List<IToggle> original, string tag)
        {
            var result = new List<IToggle>();
            if (original != null)
            {
                return original.FindAll(x => x.ToggleTag == tag);
            }
            else
            {
                return null;
            }

        }

        public void SetActionToAllToggle(Action<IToggle> action)
        {
            foreach (var item in toggles)
            {
                item.OnClick += action;
            }
        }
    }
}