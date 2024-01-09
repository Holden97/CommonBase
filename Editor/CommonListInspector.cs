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
        private void OnEnable()
        {
            itemPrefab = serializedObject.FindProperty("itemPrefab");
            useExisted = serializedObject.FindProperty("onlyUseExisted");
            itemParent = serializedObject.FindProperty("itemParent");
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
            serializedObject.ApplyModifiedProperties();

        }
    }
}

#endif
