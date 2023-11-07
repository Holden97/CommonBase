using System;
using UnityEditor;
using UnityEngine;

namespace CommonBase
{
    [RequireComponent(typeof(CanvasGroup))]
#if UNITY_EDITOR
    [CanEditMultipleObjects]
#endif
    public abstract class BaseUI : MonoBehaviour, IListener, IView
    {
        public bool IsShowing { get; private set; }
        public UIType uiLayer = UIType.PANEL;
        public int orderInLayer = 0;
        public bool ecsRemovable = true;
        /// <summary>
        /// 打开时隐藏其余底层级/同层级
        /// </summary>
        public bool coverOthersWhenShow = false;
        public PanelFadeType fadeType;
        public string UiName => GetType().Name;


        /// <summary>
        /// 进入时
        /// </summary>
        public virtual void OnEnter()
        {
            IsShowing = true;
        }

        /// <summary>
        /// 暂停时
        /// </summary>
        public virtual void OnPause() { }

        /// <summary>
        /// 继续时
        /// </summary>
        public virtual void OnResume() { }

        /// <summary>
        /// 退出时
        /// </summary>
        public virtual void OnExit()
        {
            IsShowing = false;
        }

        public virtual void Initialize()
        {
        }

        public virtual void Close()
        {
            UIManager.Instance.Hide(this.GetType(), uiLayer);
        }

        protected virtual void OnDisable()
        {
            this.EventUnregister();
        }

        /// <summary>
        /// 更新面板
        /// </summary>
        /// <param name="o"></param>
        public virtual void UpdateView(object o) { }
    }

    internal interface IView
    {
        void UpdateView(object o);
    }
}
