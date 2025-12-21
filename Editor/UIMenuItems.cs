#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace CommonBase
{
    /// <summary>
    /// UI 菜单项，用于在右键菜单中快速创建 UI 预制体
    /// </summary>
    public static class UIMenuItems
    {
        private const string UIPrefabPath = "Packages/com.commonbase/Editor/UIPrefab/";

        /// <summary>
        /// 创建 Vertical Scroll View
        /// </summary>
        [MenuItem("GameObject/UI/Vertical Scroll View", false, 2050)]
        private static void CreateVerticalScrollView(MenuCommand menuCommand)
        {
            CreateUIPrefab("Vertical Scroll View", menuCommand);
        }

        /// <summary>
        /// 通用的创建 UI 预制体方法
        /// </summary>
        /// <param name="prefabName">预制体名称</param>
        /// <param name="menuCommand">菜单命令</param>
        private static void CreateUIPrefab(string prefabName, MenuCommand menuCommand)
        {
            // 加载预制体
            string prefabPath = UIPrefabPath + prefabName + ".prefab";
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);

            if (prefab == null)
            {
                Debug.LogError($"无法加载预制体: {prefabPath}");
                return;
            }

            // 检查是否在预制体编辑模式
            var prefabStage = UnityEditor.SceneManagement.PrefabStageUtility.GetCurrentPrefabStage();
            GameObject instance;

            if (prefabStage != null)
            {
                // 在预制体编辑模式中，使用 Instantiate 而不是 InstantiatePrefab
                // 这样可以避免标记场景为修改状态
                instance = Object.Instantiate(prefab);
            }
            else
            {
                // 在普通场景中，使用 InstantiatePrefab
                instance = PrefabUtility.InstantiatePrefab(prefab) as GameObject;
            }

            if (instance == null)
            {
                Debug.LogError($"无法实例化预制体: {prefabName}");
                return;
            }

            // 获取父对象
            GameObject parent = GetParentGameObject(menuCommand);

            // 设置父级和对齐
            GameObjectUtility.SetParentAndAlign(instance, parent);

            // 设置 RectTransform 默认值（重置位置）
            RectTransform rectTransform = instance.GetComponent<RectTransform>();
            if (rectTransform != null)
            {
                rectTransform.anchoredPosition = Vector2.zero;
            }

            // 注册 Undo 操作
            Undo.RegisterCreatedObjectUndo(instance, "Create " + prefabName);

            // 选中新创建的对象
            Selection.activeObject = instance;

            // 在预制体编辑模式中标记预制体为已修改
            if (prefabStage != null)
            {
                UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(prefabStage.scene);
            }
        }

        /// <summary>
        /// 获取父对象
        /// </summary>
        private static GameObject GetParentGameObject(MenuCommand menuCommand)
        {
            GameObject parent = menuCommand.context as GameObject;

            // 检查是否在预制体编辑模式
            var prefabStage = UnityEditor.SceneManagement.PrefabStageUtility.GetCurrentPrefabStage();
            if (prefabStage != null)
            {
                // 在预制体编辑模式中
                if (parent != null)
                {
                    return parent;
                }
                // 如果没有选中对象，返回预制体根节点
                return prefabStage.prefabContentsRoot;
            }

            // 在普通场景中，返回选中的父对象（可能为 null）
            return parent;
        }

        /// <summary>
        /// 验证菜单项是否可用
        /// </summary>
        [MenuItem("GameObject/UI/Vertical Scroll View", true)]
        private static bool ValidateCreateVerticalScrollView()
        {
            // 始终可用
            return true;
        }
    }
}
#endif