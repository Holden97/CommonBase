using UnityEngine;
using DG.Tweening;

public class UIPanelAnimator : MonoBehaviour
{
    [Header("动画参数")]
    public float duration = 0.6f;               // 动画时长
    public Ease easeTypeIn = Ease.OutBack;      // 飞入的缓动曲线
    public Ease easeTypeOut = Ease.InBack;      // 飞出的缓动曲线
    public Vector2 hiddenOffset = new Vector2(0, 500); // 初始隐藏偏移量（向上）

    private RectTransform rectTransform;
    private Vector2 originalPosition;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        originalPosition = rectTransform.anchoredPosition;
    }

    /// <summary>
    /// 打开面板：自上而下飞入
    /// </summary>
    public void OpenPanel()
    {
        // gameObject.SetActive(true);
        //
        // // 设置初始位置（在目标点上方）
        // rectTransform.anchoredPosition = originalPosition + hiddenOffset;
        //
        // // 动画飞入到目标点
        // rectTransform.DOAnchorPos(originalPosition, duration)
        //     .SetEase(easeTypeIn);
    }

    /// <summary>
    /// 关闭面板：自下而上飞出
    /// </summary>
    public void ClosePanel()
    {
        // rectTransform.DoAnchorPos(originalPosition + hiddenOffset, duration)
        //     .SetEase(easeTypeOut)
        //     .OnComplete(() =>
        //     {
        //         gameObject.SetActive(false);
        //     });
    }
}