using System;
using UnityEngine;

namespace CommonBase
{
    public abstract class BaseUI : MonoBehaviour, IListener
    {
        public virtual string Path { get; }
        public bool IsShowing { get; private set; }
        public UIType uiLayer = UIType.PANEL;
        public int orderInLayer = 0;
        /// <summary>
        /// 打开时隐藏其余底层级/同层级
        /// </summary>
        public bool coverOthersWhenShow = false;
        public string UiName => GetType().Name;


        /// <summary>
        /// 进入时
        /// </summary>
        public virtual void OnEnter()
        {
            IsShowing = true;
        }

        /// <summary>
        /// 更新面板
        /// </summary>
        /// <param name="data"></param>
        public virtual void UpdateView(object data)
        {

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

        public void Initialize()
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
    }
}
