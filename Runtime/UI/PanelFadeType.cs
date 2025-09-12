using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

public enum PanelFadeType 
{
    /// <summary>
    /// 无效果
    /// </summary>
    [LabelText("无")]
    None,
    /// <summary>
    /// 中心展开
    /// </summary>
    [LabelText("折叠")]
    FOLD,
    /// <summary>
    /// 向下展开后向右展开
    /// </summary>
    [LabelText("右下方")]
    DONW_RIGHT,
}
