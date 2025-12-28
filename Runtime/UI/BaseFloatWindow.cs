using UnityEngine;

namespace CommonBase
{
    /// <summary>
    /// 浮窗基类
    /// 提供基础的浮窗功能实现
    /// </summary>
    [RequireComponent(typeof(RectTransform))]
    public abstract class BaseFloatWindow : MonoBehaviour, IFloatWindow
    {
        [Header("Float Window Settings")]
        [Tooltip("浮窗相对于目标的偏移")]
        [SerializeField] protected Vector3 defaultOffset = Vector3.zero;

        [Tooltip("是否使用世界坐标")]
        [SerializeField] protected bool useWorldPosition = false;

        [Tooltip("是否在目标不活跃时自动隐藏")]
        [SerializeField] protected bool autoHideWhenTargetInactive = true;

        [Tooltip("是否每帧更新位置")]
        [SerializeField] protected bool updatePositionEveryFrame = true;

        protected Transform attachedTarget;
        protected Vector3 offset;
        protected RectTransform rectTransform;
        protected CanvasGroup canvasGroup;
        protected bool isVisible;

        public Transform FloatWindowTransform => transform;
        public bool IsVisible => isVisible;
        public Transform AttachedTarget => attachedTarget;

        protected virtual void Awake()
        {
            rectTransform = GetComponent<RectTransform>();
            canvasGroup = GetComponent<CanvasGroup>();

            // 如果没有 CanvasGroup，添加一个
            if (canvasGroup == null)
            {
                canvasGroup = gameObject.AddComponent<CanvasGroup>();
            }

            // 初始时隐藏
            Hide();
        }

        protected virtual void Update()
        {
            if (updatePositionEveryFrame && isVisible && attachedTarget != null)
            {
                // 检查目标是否还活跃
                if (autoHideWhenTargetInactive && !attachedTarget.gameObject.activeInHierarchy)
                {
                    Hide();
                    return;
                }

                UpdatePosition();
            }
        }

        public virtual void AttachToTarget(Transform target, Vector3 offset = default)
        {
            attachedTarget = target;
            this.offset = offset == default ? defaultOffset : offset;

            if (attachedTarget != null)
            {
                UpdatePosition();
            }
        }

        public virtual void DetachFromTarget()
        {
            attachedTarget = null;
        }

        public virtual void Show(object data = null)
        {
            if (attachedTarget == null)
            {
                Debug.LogWarning($"[BaseFloatWindow] Cannot show float window '{name}' without attached target!");
                return;
            }

            gameObject.SetActive(true);
            isVisible = true;

            if (canvasGroup != null)
            {
                canvasGroup.alpha = 1f;
                canvasGroup.interactable = true;
                canvasGroup.blocksRaycasts = true;
            }

            UpdatePosition();
            OnShow(data);
        }

        public virtual void Hide()
        {
            isVisible = false;

            if (canvasGroup != null)
            {
                canvasGroup.alpha = 0f;
                canvasGroup.interactable = false;
                canvasGroup.blocksRaycasts = false;
            }

            OnHide();
        }

        public virtual void UpdatePosition()
        {
            if (attachedTarget == null || rectTransform == null)
                return;

            if (useWorldPosition)
            {
                // 使用世界坐标
                rectTransform.position = attachedTarget.position + offset;
            }
            else
            {
                // 使用UI坐标（屏幕空间）
                Vector3 screenPos = RectTransformUtility.WorldToScreenPoint(Camera.main, attachedTarget.position);
                Vector2 localPos;

                // 转换为 Canvas 的本地坐标
                Canvas canvas = GetComponentInParent<Canvas>();
                if (canvas != null && canvas.renderMode == RenderMode.ScreenSpaceOverlay)
                {
                    // Overlay 模式
                    rectTransform.position = screenPos + offset;
                }
                else if (canvas != null && canvas.renderMode == RenderMode.ScreenSpaceCamera)
                {
                    // Camera 模式
                    RectTransformUtility.ScreenPointToLocalPointInRectangle(
                        canvas.transform as RectTransform,
                        screenPos,
                        canvas.worldCamera,
                        out localPos);

                    rectTransform.localPosition = localPos + (Vector2)offset;
                }
                else
                {
                    // World Space 模式
                    rectTransform.position = attachedTarget.position + offset;
                }
            }

            OnPositionUpdated();
        }

        /// <summary>
        /// 当浮窗显示时调用（子类重写）
        /// </summary>
        protected virtual void OnShow(object data) { }

        /// <summary>
        /// 当浮窗隐藏时调用（子类重写）
        /// </summary>
        protected virtual void OnHide() { }

        /// <summary>
        /// 当位置更新时调用（子类重写）
        /// </summary>
        protected virtual void OnPositionUpdated() { }

        protected virtual void OnDestroy()
        {
            DetachFromTarget();
        }
    }
}