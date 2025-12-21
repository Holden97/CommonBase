using UnityEngine;

namespace CommonBase
{
    /// <summary>
    /// UI 小部件接口
    /// 用于标识不是完整 Panel 的 UI 组件，例如列表项、自定义控件等
    /// </summary>
    public interface IUIWidget
    {
        /// <summary>
        /// 获取小部件的 GameObject
        /// </summary>
        GameObject gameObject { get; }

        /// <summary>
        /// 获取小部件的 Transform
        /// </summary>
        Transform transform { get; }

        /// <summary>
        /// 初始化小部件
        /// </summary>
        void Initialize();

        /// <summary>
        /// 显示小部件
        /// </summary>
        void Show();

        /// <summary>
        /// 隐藏小部件
        /// </summary>
        void Hide();
    }
}