using System;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;

namespace CommonBase
{
    /// <summary>
    /// ui基类
    /// 注意，ui的事件默认会在disable时注销，所以最好不要在Awake时注册事件
    /// </summary>
    [RequireComponent(typeof(CanvasGroup))]
#if UNITY_EDITOR
    [CanEditMultipleObjects]
#endif
    public abstract class BaseUI : MonoBehaviour, IListener, IView
    {
        public bool IsShowing { get; private set; }
        [LabelText("UI层级")]
        public UIType uiLayer = UIType.PANEL;
        [LabelText("层级中次序")]
        public int orderInLayer = 0;
        [LabelText("可用Esc关闭")]
        public bool escRemovable = false;
        /// <summary>
        /// 打开时隐藏其余底层级/同层级
        /// </summary>
        [LabelText("打开时隐藏其余底层级/同层级")]
        public bool coverOthersWhenShow = false;
        [LabelText("转换动效")]
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

        /// <summary>
        /// 创建时回调，只调用一次
        /// </summary>
        public virtual void Initialize()
        {
        }

        public virtual void Close()
        {
            UIManager.Instance.Hide(this.GetType(), uiLayer);
        }

        protected virtual void OnDisable()
        {
            this.EventUnregisterAll();
        }

        /// <summary>
        /// 更新面板
        /// </summary>
        /// <param name="o"></param>
        public virtual void UpdateView(object o) { }

        public virtual GameObject AddPrefab(GameObject o)
        {
            GameObject go = GameObject.Instantiate(o);
            go.transform.parent = this.transform;
            go.transform.localPosition = Vector3.zero;
            go.transform.localRotation = Quaternion.identity;
            go.transform.localScale = Vector3.one;

            return go;
        }
    }

    internal interface IView
    {
        void UpdateView(object o);
    }
}
