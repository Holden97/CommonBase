using System.Collections.Generic;
using UnityEngine;

namespace CommonBase
{
    /// <summary>
    /// 浮窗管理器
    /// 管理所有浮窗的生命周期和显示
    /// </summary>
    public class FloatWindowManager : Singleton<FloatWindowManager>
    {
        private Dictionary<string, IFloatWindow> floatWindows = new Dictionary<string, IFloatWindow>();
        private List<IFloatWindow> activeWindows = new List<IFloatWindow>();

        /// <summary>
        /// 注册浮窗
        /// </summary>
        public void RegisterFloatWindow(string key, IFloatWindow window)
        {
            if (floatWindows.ContainsKey(key))
            {
                Debug.LogWarning($"[FloatWindowManager] Float window with key '{key}' already registered!");
                return;
            }

            floatWindows[key] = window;
            Debug.Log($"[FloatWindowManager] Registered float window: {key}");
        }

        /// <summary>
        /// 注销浮窗
        /// </summary>
        public void UnregisterFloatWindow(string key)
        {
            if (floatWindows.ContainsKey(key))
            {
                var window = floatWindows[key];
                if (activeWindows.Contains(window))
                {
                    activeWindows.Remove(window);
                    window.Hide();
                }

                floatWindows.Remove(key);
                Debug.Log($"[FloatWindowManager] Unregistered float window: {key}");
            }
        }

        /// <summary>
        /// 获取浮窗
        /// </summary>
        public IFloatWindow GetFloatWindow(string key)
        {
            if (floatWindows.TryGetValue(key, out var window))
            {
                return window;
            }

            Debug.LogWarning($"[FloatWindowManager] Float window with key '{key}' not found!");
            return null;
        }

        /// <summary>
        /// 显示浮窗
        /// </summary>
        public void ShowFloatWindow(string key, Transform target, Vector3 offset = default, object data = null)
        {
            var window = GetFloatWindow(key);
            if (window == null)
                return;

            window.AttachToTarget(target, offset);
            window.Show(data);

            if (!activeWindows.Contains(window))
            {
                activeWindows.Add(window);
            }
        }

        /// <summary>
        /// 隐藏浮窗
        /// </summary>
        public void HideFloatWindow(string key)
        {
            var window = GetFloatWindow(key);
            if (window == null)
                return;

            window.Hide();
            window.DetachFromTarget();

            if (activeWindows.Contains(window))
            {
                activeWindows.Remove(window);
            }
        }

        /// <summary>
        /// 隐藏所有浮窗
        /// </summary>
        public void HideAllFloatWindows()
        {
            foreach (var window in activeWindows.ToArray()) // 使用 ToArray 避免遍历时修改集合
            {
                window.Hide();
                window.DetachFromTarget();
            }

            activeWindows.Clear();
        }

        /// <summary>
        /// 检查并隐藏目标已销毁的浮窗
        /// </summary>
        public void CheckAndHideOrphanedWindows()
        {
            for (int i = activeWindows.Count - 1; i >= 0; i--)
            {
                var window = activeWindows[i];
                if (window.AttachedTarget == null || !window.AttachedTarget.gameObject.activeInHierarchy)
                {
                    window.Hide();
                    window.DetachFromTarget();
                    activeWindows.RemoveAt(i);
                }
            }
        }

        private void Update()
        {
            // 定期检查并清理失效的浮窗
            if (Time.frameCount % 60 == 0) // 每60帧检查一次
            {
                CheckAndHideOrphanedWindows();
            }
        }

        /// <summary>
        /// 创建浮窗实例（从预制体）
        /// </summary>
        public T CreateFloatWindow<T>(GameObject prefab, Transform parent = null) where T : Component, IFloatWindow
        {
            if (prefab == null)
            {
                Debug.LogError("[FloatWindowManager] Prefab is null!");
                return null;
            }

            GameObject instance = Object.Instantiate(prefab, parent);
            T window = instance.GetComponent<T>();

            if (window == null)
            {
                Debug.LogError($"[FloatWindowManager] Prefab does not have component of type {typeof(T)}!");
                Object.Destroy(instance);
                return null;
            }

            return window;
        }

        /// <summary>
        /// 创建并注册浮窗
        /// </summary>
        public T CreateAndRegisterFloatWindow<T>(string key, GameObject prefab, Transform parent = null) where T : Component, IFloatWindow
        {
            T window = CreateFloatWindow<T>(prefab, parent);
            if (window != null)
            {
                RegisterFloatWindow(key, window);
            }

            return window;
        }
    }
}