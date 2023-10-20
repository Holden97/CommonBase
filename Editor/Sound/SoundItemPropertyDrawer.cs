using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace CommonBase.Editor
{
    //[CustomPropertyDrawer(typeof(SoundItem))]
    public class SoundItemPropertyDrawer : PropertyDrawer
    {
        private bool isExpanded = true;

        //public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        //{
        //    if (GUILayout.Button(isExpanded ? "Collapse" : "Expand", EditorStyles.miniButton))
        //    {
        //        isExpanded = !isExpanded;
        //    }

        //    // 根据 isExpanded 的值来决定是否显示内容
        //    if (isExpanded)
        //    {
        //        // 这里放置你想要展开的内容
        //        EditorGUILayout.LabelField("Expanded Content");
        //    }


        //    EditorGUI.BeginProperty(position, label, property);

        //    // 获取 myStringField 字段
        //    SerializedProperty soundName = property.FindPropertyRelative("soundName");
        //    SerializedProperty soundClip = property.FindPropertyRelative("soundClip");
        //    SerializedProperty soundDescription = property.FindPropertyRelative("soundDescription");
        //    SerializedProperty randomPitch = property.FindPropertyRelative("randomPitch");
        //    SerializedProperty minPitch = property.FindPropertyRelative("soundPitchRandomVariationMin");
        //    SerializedProperty maxPitch = property.FindPropertyRelative("soundPitchRandomVariationMax");
        //    SerializedProperty soundVolume = property.FindPropertyRelative("soundVolume");

        //    //if (stringField != null)
        //    //{
        //    float labelWidth = EditorGUIUtility.labelWidth;
        //    //    float fieldWidth = position.width - labelWidth;

        //    Rect labelRect = new Rect(position.x, position.y, labelWidth, EditorGUIUtility.singleLineHeight * 7);
        //    //    //Rect stringFieldRect = new Rect(position.x + labelWidth, position.y, fieldWidth, EditorGUIUtility.singleLineHeight);

        //    // 显示标题
        //    EditorGUILayout.BeginHorizontal();
        //    //    //EditorGUI.LabelField(labelRect, label);
        //    //    //EditorGUILayout.PropertyField(stringField);
        //    //    //EditorGUILayout.EndHorizontal();

        //    //    // 显示字符串字段
        //    //    //EditorGUI.PropertyField(stringFieldRect, stringField, GUIContent.none);
        //    //}

        //    EditorGUI.EndProperty();


        //}

        //public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        //{
        //    return EditorGUIUtility.singleLineHeight * 7;
        //}

    }
}

