#if UNITY_EDITOR

//使用utf-8
using UnityEditor;
using UnityEngine;

namespace CommonBase.Editor
{
    [CustomPropertyDrawer(typeof(FiniteStateMachine))]
    public class FiniteStateMachinePropertyDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.DrawRect(position, Color.black);
            EditorGUI.LabelField(position, "状态机当前状态:" + property.FindPropertyRelative("curState").FindPropertyRelative("stateName").stringValue);
            position.y += EditorGUIUtility.singleLineHeight;
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUIUtility.singleLineHeight;
        }
    }
}
#endif
