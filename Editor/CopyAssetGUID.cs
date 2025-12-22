#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace CommonBase.Editor
{
    public static class CopyAssetGUID
    {
        [MenuItem("Assets/Copy GUID", false, 20)]
        private static void CopyGUID()
        {
            // 获取当前选中的对象
            Object selectedObject = Selection.activeObject;
            if (selectedObject == null)
            {
                Debug.LogWarning("没有选中任何资源");
                return;
            }

            // 获取资源路径
            string assetPath = AssetDatabase.GetAssetPath(selectedObject);
            if (string.IsNullOrEmpty(assetPath))
            {
                Debug.LogWarning("无法获取资源路径");
                return;
            }

            // 获取GUID
            string guid = AssetDatabase.AssetPathToGUID(assetPath);
            if (string.IsNullOrEmpty(guid))
            {
                Debug.LogWarning("无法获取资源GUID");
                return;
            }

            // 复制到剪贴板
            EditorGUIUtility.systemCopyBuffer = guid;
            Debug.Log($"已复制GUID: {guid}\n资源路径: {assetPath}");
        }

        // 验证菜单项是否可用
        [MenuItem("Assets/Copy GUID", true)]
        private static bool ValidateCopyGUID()
        {
            // 只有选中了资源时才显示菜单项
            return Selection.activeObject != null;
        }

        // 添加快捷键版本 Ctrl+Shift+G
        [MenuItem("Assets/Copy GUID (with Info) %#g", false, 21)]
        private static void CopyGUIDWithInfo()
        {
            Object selectedObject = Selection.activeObject;
            if (selectedObject == null)
            {
                Debug.LogWarning("没有选中任何资源");
                return;
            }

            string assetPath = AssetDatabase.GetAssetPath(selectedObject);
            if (string.IsNullOrEmpty(assetPath))
            {
                Debug.LogWarning("无法获取资源路径");
                return;
            }

            string guid = AssetDatabase.AssetPathToGUID(assetPath);
            if (string.IsNullOrEmpty(guid))
            {
                Debug.LogWarning("无法获取资源GUID");
                return;
            }

            // 创建包含更多信息的文本
            string info = $"Asset: {selectedObject.name}\nPath: {assetPath}\nGUID: {guid}";

            // 复制到剪贴板
            EditorGUIUtility.systemCopyBuffer = info;
            Debug.Log($"已复制资源信息:\n{info}");
        }

        [MenuItem("Assets/Copy GUID (with Info) %#g", true)]
        private static bool ValidateCopyGUIDWithInfo()
        {
            return Selection.activeObject != null;
        }

        // 批量复制多个选中资源的GUID
        [MenuItem("Assets/Copy All Selected GUIDs", false, 22)]
        private static void CopyAllGUIDs()
        {
            Object[] selectedObjects = Selection.objects;
            if (selectedObjects == null || selectedObjects.Length == 0)
            {
                Debug.LogWarning("没有选中任何资源");
                return;
            }

            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            int count = 0;

            foreach (Object obj in selectedObjects)
            {
                string assetPath = AssetDatabase.GetAssetPath(obj);
                if (string.IsNullOrEmpty(assetPath))
                    continue;

                string guid = AssetDatabase.AssetPathToGUID(assetPath);
                if (string.IsNullOrEmpty(guid))
                    continue;

                sb.AppendLine($"{obj.name}: {guid}");
                count++;
            }

            if (count > 0)
            {
                EditorGUIUtility.systemCopyBuffer = sb.ToString();
                Debug.Log($"已复制 {count} 个资源的GUID:\n{sb.ToString()}");
            }
            else
            {
                Debug.LogWarning("没有找到有效的资源GUID");
            }
        }

        [MenuItem("Assets/Copy All Selected GUIDs", true)]
        private static bool ValidateCopyAllGUIDs()
        {
            return Selection.objects != null && Selection.objects.Length > 0;
        }
    }
}
#endif