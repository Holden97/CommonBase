#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace CommonBase.Editor
{
    [CustomPropertyDrawer(typeof(ReadOnlyFieldAttribute))]
    internal class ReadOnlyPropertyDrawer : PropertyDrawer
    {
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUI.GetPropertyHeight(property);
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            //Using BeginProperty/EndProperty on the parent property means that prefab override logic works on the entire property.

            EditorGUI.BeginProperty(position, label, property);

            if (property.propertyType == SerializedPropertyType.Integer)
            {
                EditorGUI.BeginChangeCheck();
                //Draw item code
                //var newValue = EditorGUI.IntField(new Rect(position.x, position.y, position.width, position.height / 2), label, property.intValue);

                //Draw item description
                EditorGUI.LabelField(new Rect(position.x, position.y + position.height / 2, position.width, position.height / 2), property.name, property.intValue.ToString());

                //if (EditorGUI.EndChangeCheck())
                //{
                //    property.intValue = newValue;
                //}
            }


            EditorGUI.EndProperty();
        }
    }
}
#endif