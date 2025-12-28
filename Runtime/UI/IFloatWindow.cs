
using UnityEngine;

namespace CommonBase
{
    /// <summary>
    /// 浮窗接口
    /// 浮窗是一种特殊的UI元素，用于悬浮信息展示
    /// 特点：跟随父节点，当父节点不活跃时自动隐藏
    /// </summary>
    public interface IFloatWindow
    {
        /// <summary>
        /// 浮窗的 Transform
        /// </summary>
        Transform FloatWindowTransform { get; }

        /// <summary>
        /// 绑定到目标节点
        /// </summary>
        /// <param name="target">要跟随的目标</param>
        /// <param name="offset">相对于目标的偏移</param>
        void AttachToTarget(Transform target, Vector3 offset = default);

        /// <summary>
        /// 解除绑定
        /// </summary>
        void DetachFromTarget();

        /// <summary>
        /// 显示浮窗
        /// </summary>
        /// <param name="data">要显示的数据</param>
        void Show(object data = null);

        /// <summary>
        /// 隐藏浮窗
        /// </summary>
        void Hide();

        /// <summary>
        /// 更新浮窗位置（跟随目标）
        /// </summary>
        void UpdatePosition();

        /// <summary>
        /// 浮窗是否可见
        /// </summary>
        bool IsVisible { get; }

        /// <summary>
        /// 当前绑定的目标
        /// </summary>
        Transform AttachedTarget { get; }
    }
}