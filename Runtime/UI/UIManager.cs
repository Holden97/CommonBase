﻿using DG.Tweening;
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Rendering;
using UnityEngine.UI;

namespace CommonBase
{
    public class UIManager : MonoSingleton<UIManager>
    {
        public string PANEL_ROOT;
        private Dictionary<UIType, StackPro<BaseUI>> uiDic;
        private Stack<UIShowInfoList> uiInfoStack;

        protected GameObject parent;
        public GameObject tip;
        public GameObject UICanvas { get; protected set; }

        private int showingPanelCount;
        /// <summary>
        /// 正在显示panel，不响应游戏中的事件。
        /// </summary>
        public bool IsShowingPanel => showingPanelCount > 0;

        public UIManager()
        {

            uiDic = new Dictionary<UIType, StackPro<BaseUI>>();
            uiInfoStack = new Stack<UIShowInfoList>();

            uiDic.Add(UIType.CANVAS, new StackPro<BaseUI>());
            uiDic.Add(UIType.PANEL, new StackPro<BaseUI>());
        }

        protected override void Awake()
        {
            base.Awake();
            tip = Resources.Load<GameObject>("Tip");
            parent = GameObject.FindGameObjectWithTag("UICanvas");
            if (parent != null)
            {
                UICanvas = parent;
            }
            else
            {
                UICanvas = GameObject.Instantiate(Resources.Load<GameObject>("UICanvas"));
                tip.SetActive(false);
            }
            UICanvas.transform.SetParent(null);
            //this.EventRegister<string>(EventName.CHANGE_SCENE, OnSceneChange);
            DontDestroyOnLoad(UICanvas);
            UICanvas.GetComponent<Canvas>().sortingOrder = 1;
            //检查是否创建了EventSystem
            if (GameObject.FindObjectOfType<EventSystem>() == null)
            {
                Debug.LogError("在当前场景中并未寻找到EventSystem，请检查");
            }
            Debug.Log("uiManager 初始化结束");
        }

        private void OnSceneChange(string arg0)
        {
            HideAll(true);
        }

        public BaseUI Find(int uiInstanceId)
        {
            foreach (var uiList in uiDic)
            {
                foreach (var ui in uiList.Value)
                {
                    if (ui.GetInstanceID() == uiInstanceId)
                    {
                        return ui;
                    }
                }
            }
            return null;
        }

        public T ShowPanel<T>(Action<T> OnShow = null, string path = null, object data = null) where T : BaseUI, new()
        {
            string realPath = path != null ? path : $"{PANEL_ROOT + "/" + typeof(T).Name}";
            var p = Show(UIType.PANEL, realPath, OnShow);
            if (data != null)
            {
                p.UpdateView(data);
            }
            return p;
        }

        public void ShowTip(string content)
        {
            var go = Instantiate(tip);
            var cg = go.GetComponentInChildren<CanvasGroup>();
            go.SetActive(true);
            go.transform.SetParent(null);
            go.GetComponentInChildren<Text>().text = content;
            var startPoint = cg.transform.position.y;
            cg.transform.DOMoveY(startPoint + 50, 0.5f).OnComplete(() =>
            {
                DOTween.To(() => cg.alpha, value => cg.alpha = value, 0, 1)
                .SetEase(Ease.Linear)
                .OnComplete(() =>
                {
                    Destroy(go);
                });
            });
        }

        public T Find<T>() where T : BaseUI, new()
        {
            if (!parent)
            {
                Debug.LogError("Canvas is null!");
                return null;
            }
            foreach (var uiType in uiDic)
            {
                foreach (var item in uiType.Value)
                {
                    if (item.GetType().Name == typeof(T).Name)
                    {
                        return item as T;
                    }
                }
            }
            return null;

        }

        public void UpdatePanel<T>(object data) where T : BaseUI, new()
        {
            var ui = Find<T>();
            if (ui != null)
            {
                ui.UpdateView(data);
            }
            else
            {
                Debug.LogError($"未找到{typeof(T)}类型的UI！");
            }
        }

        public T LoadPanel<T>(string path = null, object data = null) where T : BaseUI, new()
        {
            string realPath = path != null ? path : $"{typeof(T).Name}";
            var p = Get<T>(UIType.PANEL, realPath);
            return p;
        }

        public T Get<T>(UIType uiType, string path) where T : BaseUI, new()
        {
            if (!parent)
            {
                Debug.LogError("Canvas is null!");
                return null;
            }

            BaseUI uiToShow = default;
            //先从缓存字典中查找
            foreach (var item in uiDic[uiType])
            {
                if (item.GetType().Name == typeof(T).Name)
                {
                    uiToShow = item;
                    break;
                }
            }
            //如果没有，则创建
            if (uiToShow == null)
            {
                GameObject uiObject = GameObject.Instantiate(Resources.Load<GameObject>(path), parent.transform);
                uiObject.SetActive(false);
                uiToShow = uiObject.GetComponent<BaseUI>();
                if (uiToShow == null)
                {
                    Debug.LogError("创建的UI并没有挂载BaseUI组件!");
                }
                else
                {
                    uiObject.name = uiToShow.UiName;
                }
                uiDic[uiType].Push(uiToShow);
            }

            //如果需要覆盖其他面板，则覆盖
            if (uiToShow.coverOthersWhenShow)
            {
                uiInfoStack.Push(new UIShowInfoList(uiDic[uiType]));
                foreach (var ui in uiDic[uiType])
                {
                    if (ui.orderInLayer <= uiToShow.orderInLayer && ui != uiToShow)
                        if (ui.orderInLayer <= uiToShow.orderInLayer && ui != uiToShow)
                        {
                            HideInside(ui, false);
                        }
                }
            }

            return uiToShow as T;

        }

        public T Get<T>() where T : BaseUI, new()
        {
            BaseUI uiToShow = default;
            //从缓存字典中查找
            foreach (var uiType in uiDic)
            {
                foreach (var item in uiType.Value)
                {
                    if (item.GetType().Name == typeof(T).Name)
                    {
                        uiToShow = item;
                        break;
                    }
                }
            }
            return uiToShow as T;

        }


        public T Show<T>(UIType uiType, string path, Action<T> OnShow = null) where T : BaseUI, new()
        {
            string realPath = path != null ? path : $"{typeof(T).Name}";
            var uiToShow = Get<T>(uiType, realPath);
            Show(uiToShow, OnShow);
            return uiToShow as T;
        }

        public void Show<T>(BaseUI uiToShow, Action<T> OnShow = null) where T : BaseUI, new()
        {
            var i = uiToShow.transform.parent.childCount - 1;
            BaseUI curUI = default;
            for (; i >= 0; i--)
            {
                curUI = uiToShow.transform.parent.GetChild(i).GetComponent<BaseUI>();
                if (curUI==null)
                {
                    Debug.LogError($"{uiToShow.transform.parent.GetChild(i).name}上未挂载BaseUI组件，请检查！");
                }
                if (curUI.orderInLayer <= uiToShow.orderInLayer && curUI != uiToShow)
                {
                    var curIndex = uiToShow.transform.GetSiblingIndex();
                    if (curIndex < i)
                    {
                        uiToShow.transform.SetSiblingIndex(i);
                    }
                    else
                    {
                        uiToShow.transform.SetSiblingIndex(i + 1);
                    }
                    break;
                }
            }
            if (i < 0)
            {
                uiToShow.transform.SetAsFirstSibling();
            }
            uiToShow.gameObject.SetActive(true);
            uiToShow.OnEnter();
            OnShow?.Invoke(uiToShow as T);
            SetManagerProperty(uiToShow.uiLayer);
        }

        private void SetManagerProperty(UIType uiType)
        {
            switch (uiType)
            {
                case UIType.CANVAS:
                    break;
                case UIType.PANEL:
                    showingPanelCount = 0;
                    foreach (var item in uiDic[uiType])
                    {
                        if (item.IsShowing)
                        {
                            showingPanelCount++;
                        }
                    }
                    break;
                default:
                    break;
            }
        }

        public void Hide(Type type, UIType uiType, bool destroyIt = false)
        {
            this.GetType().GetMethod("HideInside").MakeGenericMethod(type).Invoke(this, new object[]
            {
            uiType,
            destroyIt,
            false
            });
        }

        public void Hide<T>(bool destroyIt = false, bool recover = false)
        {
            BaseUI item = default(BaseUI);
            foreach (var uiType in uiDic)
            {
                for (int i = 0; i < uiType.Value.Count; i++)
                {
                    item = uiType.Value[i];
                    if (item.GetType() == typeof(T))
                    {
                        HideInside(item, destroyIt);
                        break;
                    }
                }
            }

            if (recover && item != null && item.coverOthersWhenShow)
            {
                Recover();
            }
        }

        private void Recover()
        {
            UIShowInfoList infoList = uiInfoStack.Pop();
            foreach (var ui in infoList.uiInfo)
            {
                if (ui.isShowing)
                {
                    var curUI = Find(ui.instanceId);
                    MethodInfo method = typeof(UIManager).GetMethod("ShowInside", BindingFlags.Instance | BindingFlags.NonPublic).MakeGenericMethod(curUI.GetType());
                    method.Invoke(this, new object[] { curUI, null });
                }
            }
        }

        public void HideInside<T>(UIType uiType, bool destroyIt = false, bool recover = false) where T : BaseUI
        {
            BaseUI item = default(BaseUI);
            for (int i = 0; i < uiDic[uiType].Count; i++)
            {
                item = uiDic[uiType][i];
                if (item.GetType() == typeof(T))
                {
                    HideInside(item, destroyIt);
                    break;
                }
            }

            if (recover && item != null && item.coverOthersWhenShow)
            {
                Recover();
            }
        }

        private void HideInside(BaseUI item, bool destroyIt = false)
        {
            //var lastChildIndex = item.transform.parent.childCount - 1;
            //var parent = item.transform.parent;
            item.OnExit();
            if (destroyIt)
            {
                uiDic[item.uiLayer].Remove(item);
                Destroy(item.gameObject);
                //lastChildIndex--;
            }
            else
            {
                item.gameObject.SetActive(false);
            }
            //执行一次目前type最上面一个正在showing的UI的OnEnter方法
            //parent.GetChild(lastChildIndex).GetComponent<BaseUI>().OnEnter();
            SetManagerProperty(item.uiLayer);
        }

        public void HideAll(UIType uiType, bool destroyIt = false)
        {
            for (int i = uiDic[uiType].Count - 1; i >= 0; i--)
            {
                BaseUI item = uiDic[uiType][i];
                item.OnExit();
                if (destroyIt)
                {
                    uiDic[uiType].Remove(item);
                    item.EventUnregister();
                    Destroy(item.gameObject);
                }
                else
                {
                    item.gameObject.SetActive(false);
                }
                SetManagerProperty(uiType);
            }
        }


        public void HideAll(bool destroyIt = false)
        {
            foreach (var item in uiDic)
            {
                HideAll(item.Key, destroyIt);
            }
        }

        public void Switch<T>(UIType uiType, string path, Action<T> OnShow = null, bool recover = false) where T : BaseUI, new()
        {
            if (IsShowing<T>(uiType))
            {
                HideInside<T>(uiType, recover: recover);
            }
            else
            {
                Show<T>(uiType, path, OnShow);
            }
        }

        private bool IsShowing<T>(UIType uiType) where T : BaseUI
        {
            foreach (var item in uiDic[uiType])
            {
                if (item is T)
                {
                    return item.IsShowing;
                }
            }
            return false;
        }

        private void OnGUI()
        {
        }
    }
    public enum UIType
    {
        /// <summary>
        /// 画布
        /// </summary>
        CANVAS,
        /// <summary>
        /// 面板，一种面板只允许出现一个，但同一时间可以出现多种面板
        /// </summary>
        PANEL,
        /// <summary>
        /// 面板中的组件
        /// </summary>
        WIDGET,
    }

    public enum RectTransformAnchor
    {
        TOP_LEFT,
        BOTTOM_LEFT,
    }
}

