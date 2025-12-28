using UnityEngine;
using TMPro;

namespace CommonBase
{
    /// <summary>
    /// 工具提示浮窗
    /// 用于显示简单的文本提示信息
    /// </summary>
    public class TooltipFloatWindow : BaseFloatWindow
    {
        [Header("Tooltip Components")]
        [SerializeField] private TextMeshProUGUI txtTooltip;
        [SerializeField] private RectTransform background;

        [Header("Tooltip Settings")]
        [SerializeField] private float padding = 10f;
        [SerializeField] private float minWidth = 100f;
        [SerializeField] private float maxWidth = 400f;

        protected override void Awake()
        {
            base.Awake();

            // 查找组件
            if (txtTooltip == null)
            {
                txtTooltip = GetComponentInChildren<TextMeshProUGUI>();
            }

            if (background == null)
            {
                background = GetComponent<RectTransform>();
            }
        }

        protected override void OnShow(object data)
        {
            base.OnShow(data);

            if (data is string text)
            {
                SetText(text);
            }
            else if (data != null)
            {
                SetText(data.ToString());
            }
        }

        /// <summary>
        /// 设置提示文本
        /// </summary>
        public void SetText(string text)
        {
            if (txtTooltip == null)
            {
                Debug.LogWarning("[TooltipFloatWindow] Text component not found!");
                return;
            }

            txtTooltip.text = text;

            // 自动调整大小
            AdjustSize();
        }

        /// <summary>
        /// 根据文本内容自动调整浮窗大小
        /// </summary>
        private void AdjustSize()
        {
            if (txtTooltip == null || background == null)
                return;

            // 强制更新文本布局
            txtTooltip.ForceMeshUpdate();

            // 获取文本实际大小
            Vector2 textSize = txtTooltip.GetRenderedValues(false);

            // 计算背景大小（加上padding）
            float width = Mathf.Clamp(textSize.x + padding * 2, minWidth, maxWidth);
            float height = textSize.y + padding * 2;

            background.sizeDelta = new Vector2(width, height);
        }

        /// <summary>
        /// 快速显示工具提示
        /// </summary>
        public static void ShowTooltip(Transform target, string text, Vector3 offset = default)
        {
            var window = FloatWindowManager.Instance.GetFloatWindow("Tooltip") as TooltipFloatWindow;
            if (window == null)
            {
                Debug.LogWarning("[TooltipFloatWindow] Tooltip window not registered!");
                return;
            }

            window.AttachToTarget(target, offset);
            window.Show(text);
        }

        /// <summary>
        /// 隐藏工具提示
        /// </summary>
        public static void HideTooltip()
        {
            FloatWindowManager.Instance.HideFloatWindow("Tooltip");
        }
    }
}