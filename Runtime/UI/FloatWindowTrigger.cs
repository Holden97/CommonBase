using UnityEngine;
using UnityEngine.EventSystems;

namespace CommonBase
{
    /// <summary>
    /// 浮窗触发器
    /// 用于在UI元素上触发浮窗显示/隐藏
    /// </summary>
    public class FloatWindowTrigger : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        [Header("Float Window Settings")]
        [Tooltip("要显示的浮窗的键")]
        [SerializeField] private string floatWindowKey = "Tooltip";

        [Tooltip("浮窗相对于此对象的偏移")]
        [SerializeField] private Vector3 offset = new Vector3(0, 50, 0);

        [Tooltip("触发方式")]
        [SerializeField] private TriggerMode triggerMode = TriggerMode.Hover;

        [Header("Content")]
        [Tooltip("要显示的内容（文本）")]
        [SerializeField] [TextArea(3, 10)] private string content = "";

        [Tooltip("显示延迟（秒）")]
        [SerializeField] private float showDelay = 0.5f;

        [Tooltip("隐藏延迟（秒）")]
        [SerializeField] private float hideDelay = 0.1f;

        private bool isPointerOver = false;
        private float hoverTime = 0f;
        private bool isShowing = false;

        public enum TriggerMode
        {
            Hover,      // 鼠标悬停
            Click,      // 点击
            Manual      // 手动控制
        }

        private void Update()
        {
            if (triggerMode == TriggerMode.Hover)
            {
                if (isPointerOver && !isShowing)
                {
                    hoverTime += Time.deltaTime;
                    if (hoverTime >= showDelay)
                    {
                        ShowFloatWindow();
                    }
                }
                else if (!isPointerOver && isShowing)
                {
                    HideFloatWindow();
                }
            }
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            isPointerOver = true;
            hoverTime = 0f;
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            isPointerOver = false;
            hoverTime = 0f;
        }

        /// <summary>
        /// 显示浮窗
        /// </summary>
        public void ShowFloatWindow()
        {
            if (string.IsNullOrEmpty(floatWindowKey))
            {
                Debug.LogWarning("[FloatWindowTrigger] Float window key is not set!");
                return;
            }

            FloatWindowManager.Instance.ShowFloatWindow(floatWindowKey, transform, offset, content);
            isShowing = true;
        }

        /// <summary>
        /// 隐藏浮窗
        /// </summary>
        public void HideFloatWindow()
        {
            if (string.IsNullOrEmpty(floatWindowKey))
                return;

            FloatWindowManager.Instance.HideFloatWindow(floatWindowKey);
            isShowing = false;
        }

        /// <summary>
        /// 设置显示内容
        /// </summary>
        public void SetContent(string newContent)
        {
            content = newContent;

            // 如果正在显示，更新内容
            if (isShowing)
            {
                var window = FloatWindowManager.Instance.GetFloatWindow(floatWindowKey);
                if (window != null)
                {
                    window.Show(content);
                }
            }
        }

        /// <summary>
        /// 设置浮窗键
        /// </summary>
        public void SetFloatWindowKey(string key)
        {
            floatWindowKey = key;
        }

        /// <summary>
        /// 设置偏移
        /// </summary>
        public void SetOffset(Vector3 newOffset)
        {
            offset = newOffset;
        }

        private void OnDisable()
        {
            // 组件禁用时隐藏浮窗
            if (isShowing)
            {
                HideFloatWindow();
            }

            isPointerOver = false;
            hoverTime = 0f;
        }
    }
}