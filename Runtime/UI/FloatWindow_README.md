# FloatWindow System 浮窗系统

## 概述

浮窗系统用于创建跟随父节点的悬浮UI元素，常用于工具提示、信息面板等场景。

## 核心组件

### 1. IFloatWindow 接口
定义浮窗的基本行为。

```csharp
public interface IFloatWindow
{
    Transform FloatWindowTransform { get; }
    void AttachToTarget(Transform target, Vector3 offset = default);
    void DetachFromTarget();
    void Show(object data = null);
    void Hide();
    void UpdatePosition();
    bool IsVisible { get; }
    Transform AttachedTarget { get; }
}
```

### 2. BaseFloatWindow 基类
浮窗的基础实现，提供：
- 自动跟随目标
- 目标不活跃时自动隐藏
- 位置更新（支持世界坐标和UI坐标）
- CanvasGroup控制显示/隐藏

**配置项：**
- `defaultOffset`: 默认偏移量
- `useWorldPosition`: 是否使用世界坐标
- `autoHideWhenTargetInactive`: 目标不活跃时自动隐藏
- `updatePositionEveryFrame`: 是否每帧更新位置

### 3. FloatWindowManager 管理器
管理所有浮窗的生命周期。

**主要方法：**
```csharp
// 注册浮窗
FloatWindowManager.Instance.RegisterFloatWindow("MyWindow", window);

// 显示浮窗
FloatWindowManager.Instance.ShowFloatWindow("MyWindow", target, offset, data);

// 隐藏浮窗
FloatWindowManager.Instance.HideFloatWindow("MyWindow");

// 隐藏所有浮窗
FloatWindowManager.Instance.HideAllFloatWindows();

// 创建浮窗实例
var window = FloatWindowManager.Instance.CreateFloatWindow<MyFloatWindow>(prefab, parent);

// 创建并注册
var window = FloatWindowManager.Instance.CreateAndRegisterFloatWindow<MyFloatWindow>("key", prefab, parent);
```

### 4. TooltipFloatWindow 示例实现
工具提示浮窗，支持：
- 显示文本内容
- 自动调整大小
- 静态方法快速调用

```csharp
// 显示提示
TooltipFloatWindow.ShowTooltip(target, "这是提示文本", new Vector3(0, 50, 0));

// 隐藏提示
TooltipFloatWindow.HideTooltip();
```

### 5. FloatWindowTrigger 触发器
UI辅助组件，支持：
- 鼠标悬停触发
- 点击触发
- 手动控制
- 延迟显示/隐藏

## 使用示例

### 创建自定义浮窗

```csharp
using CommonBase;
using UnityEngine;
using TMPro;

public class MyInfoFloatWindow : BaseFloatWindow
{
    [SerializeField] private TextMeshProUGUI txtTitle;
    [SerializeField] private TextMeshProUGUI txtDescription;

    protected override void OnShow(object data)
    {
        base.OnShow(data);

        if (data is MyData myData)
        {
            txtTitle.text = myData.title;
            txtDescription.text = myData.description;
        }
    }

    protected override void OnHide()
    {
        base.OnHide();
        // 清理逻辑
    }
}
```

### 注册和使用浮窗

```csharp
// 在某个管理器初始化时
void Initialize()
{
    // 从预制体创建并注册
    var window = FloatWindowManager.Instance.CreateAndRegisterFloatWindow<MyInfoFloatWindow>(
        "InfoWindow",
        infoPrefab,
        uiRoot
    );
}

// 显示浮窗
void ShowInfo(Transform target, MyData data)
{
    FloatWindowManager.Instance.ShowFloatWindow(
        "InfoWindow",
        target,
        new Vector3(0, 100, 0),
        data
    );
}

// 隐藏浮窗
void HideInfo()
{
    FloatWindowManager.Instance.HideFloatWindow("InfoWindow");
}
```

### 使用 FloatWindowTrigger

1. 在UI元素上添加 `FloatWindowTrigger` 组件
2. 设置：
   - Float Window Key: "Tooltip"
   - Offset: (0, 50, 0)
   - Trigger Mode: Hover
   - Content: "这是提示文本"
   - Show Delay: 0.5

现在鼠标悬停在该UI元素上0.5秒后会自动显示浮窗。

### 工具提示快速使用

```csharp
using UnityEngine.EventSystems;

public class MyButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public void OnPointerEnter(PointerEventData eventData)
    {
        TooltipFloatWindow.ShowTooltip(transform, "点击购买物品", new Vector3(0, 50, 0));
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        TooltipFloatWindow.HideTooltip();
    }
}
```

## 最佳实践

1. **初始化时注册浮窗**
   - 在游戏开始或UI初始化时注册所有浮窗
   - 避免运行时频繁创建销毁

2. **使用对象池**
   - 对于频繁使用的浮窗，建议使用对象池
   - 可以继承 `BaseFloatWindow` 并实现对象池逻辑

3. **注意Canvas模式**
   - Overlay模式：简单直接
   - Camera模式：需要设置合适的相机
   - World Space：适合3D场景中的UI

4. **性能优化**
   - 不需要每帧更新位置的浮窗，设置 `updatePositionEveryFrame = false`
   - 使用 `autoHideWhenTargetInactive = true` 自动清理失效浮窗

5. **事件清理**
   - 在 `OnDisable` 或 `OnDestroy` 中记得清理浮窗

## 注意事项

- 浮窗需要在Canvas下创建
- 确保 FloatWindowManager 已初始化（Singleton会自动创建）
- 注册的浮窗键（key）必须唯一
- 目标对象被销毁时，浮窗会自动隐藏（如果开启了autoHideWhenTargetInactive）