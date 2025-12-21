#if UNITY_EDITOR
//使用utf-8
using UnityEditor;
using UnityEngine;

namespace CommonBase.Editor
{

    [CustomEditor(typeof(CommonList))]
    public class CommonListInspector : UnityEditor.Editor
    {
        private SerializedProperty itemPrefab;
        private SerializedProperty useExisted;
        private SerializedProperty itemParent;
        private SerializedProperty destroyExistedItemsOnRuntime;

        private int itemCountToAdd = 1;

        private void OnEnable()
        {
            itemPrefab = serializedObject.FindProperty("itemPrefab");
            useExisted = serializedObject.FindProperty("onlyUseExisted");
            itemParent = serializedObject.FindProperty("itemParent");
            destroyExistedItemsOnRuntime = serializedObject.FindProperty("destroyExistedItemsOnRuntime");
        }

        public override void OnInspectorGUI()
        {
            CommonList list = (CommonList)target;
            EditorGUILayout.PropertyField(useExisted, new GUIContent("只使用列表中已存在的预设"));
            if (!useExisted.boolValue)
            {
                EditorGUILayout.PropertyField(itemPrefab, new GUIContent("单元预制体"));
            }
            EditorGUILayout.PropertyField(itemParent, new GUIContent("单元父节点"));

            EditorGUILayout.Space();
            EditorGUILayout.PropertyField(destroyExistedItemsOnRuntime, new GUIContent("运行时删除预制体中的 Item", "开启后，运行时会删除（Destroy）预制体中已存在的子 Item，而不是仅仅隐藏它们"));

            serializedObject.ApplyModifiedProperties();

            // 编辑器工具按钮区域
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("编辑器工具", EditorStyles.boldLabel);

            // 添加 Item 按钮
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("添加数量", GUILayout.Width(60));
            itemCountToAdd = EditorGUILayout.IntField(itemCountToAdd, GUILayout.Width(50));
            itemCountToAdd = Mathf.Max(1, itemCountToAdd); // 确保至少为 1

            GUI.enabled = list.itemPrefab != null && list.itemParent != null;
            if (GUILayout.Button($"添加 {itemCountToAdd} 个 Item", GUILayout.ExpandWidth(true)))
            {
                AddItems(list, itemCountToAdd);
            }
            GUI.enabled = true;
            EditorGUILayout.EndHorizontal();

            // 清除所有 Item 按钮
            GUI.enabled = list.itemParent != null && list.itemParent.childCount > 0;
            var originalColor = GUI.backgroundColor;
            GUI.backgroundColor = new Color(1f, 0.6f, 0.6f); // 淡红色背景
            if (GUILayout.Button("清除所有 Item"))
            {
                ClearAllItems(list);
            }
            GUI.backgroundColor = originalColor;
            GUI.enabled = true;
        }

        /// <summary>
        /// 添加指定数量的 Item 到父节点下
        /// </summary>
        private void AddItems(CommonList list, int count)
        {
            if (list.itemPrefab == null)
            {
                EditorUtility.DisplayDialog("错误", "请先指定单元预制体", "确定");
                return;
            }

            if (list.itemParent == null)
            {
                EditorUtility.DisplayDialog("错误", "请先指定单元父节点", "确定");
                return;
            }

            Undo.SetCurrentGroupName($"Add {count} Items");
            int undoGroup = Undo.GetCurrentGroup();

            for (int i = 0; i < count; i++)
            {
                GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(list.itemPrefab, list.itemParent);
                if (instance != null)
                {
                    instance.name = $"{list.itemPrefab.name} ({list.itemParent.childCount - 1})";
                    Undo.RegisterCreatedObjectUndo(instance, $"Add Item {i + 1}");
                }
            }

            Undo.CollapseUndoOperations(undoGroup);
            EditorUtility.SetDirty(list.itemParent);
            Debug.Log($"✅ 已添加 {count} 个 Item 到 {list.itemParent.name}");
        }

        /// <summary>
        /// 清除所有 Item
        /// </summary>
        private void ClearAllItems(CommonList list)
        {
            if (list.itemParent == null) return;

            int childCount = list.itemParent.childCount;

            Undo.SetCurrentGroupName("Clear All Items");
            int undoGroup = Undo.GetCurrentGroup();

            // 倒序删除，避免索引问题
            for (int i = childCount - 1; i >= 0; i--)
            {
                Transform child = list.itemParent.GetChild(i);
                Undo.DestroyObjectImmediate(child.gameObject);
            }

            Undo.CollapseUndoOperations(undoGroup);
            EditorUtility.SetDirty(list.itemParent);
            Debug.Log($"✅ 已清除 {childCount} 个 Item");
        }
    }
}

#endif
