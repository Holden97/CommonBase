//使用utf-8
using System;
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

        public Sprite toggleSelected;
        public Sprite toggleUnselected;
        public string groupTag;

        public IToggle First
        {
            get
            {
                if (toggles != null && toggles.Count > 0)
                {
                    return toggles[0];
                }
                return null;
            }
        }

        public void SelectToggle(IToggle toggle)
        {
            var img = (toggle as Button).transform.GetComponent<Image>();

            toggle.IsToggle = true;
            if (img != null && toggleSelected != null)
            {
                img.sprite = toggleSelected;
            }
            toggle.OnToggleSelect();
            foreach (IToggle t in toggles)
            {
                if (t != toggle)
                {
                    var otherImg = (t as Button).transform.GetComponent<Image>();
                    if (otherImg != null && toggleUnselected != null)
                    {
                        otherImg.sprite = toggleUnselected;
                        t.OnToggleUnselect();
                    }
                    t.IsToggle = false;
                }
            }
        }

        /// <summary>
        /// 初始化
        /// 初始化顺序:toggles数据确定=>为toggles绑定回调
        /// </summary>
        public void Initialize()
        {
            toggles = SetToggles(groupTag);
            foreach (var t in toggles)
            {
                t.OnClickToggle = SelectToggle;
            }
            SelectFirst();
        }

        public void SelectFirst()
        {
            if (toggles.Count > 0)
            {
                toggles[0].OnClickToggle(toggles[0]);
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

        public void SetOnSelectedToAll(UnityAction<IToggle> action)
        {
            foreach (var t in toggles)
            {
                t.AddSelectedAction(action);
            }
        }

        public void SetOnUnselectedToAll(UnityAction<IToggle> action)
        {
            foreach (var item in toggles)
            {
                item.AddUnselectedAction(action);
            }
        }
    }
}