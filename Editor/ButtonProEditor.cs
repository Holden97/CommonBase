﻿#if UNITY_EDITOR

using UnityEditor;
[CustomEditor(typeof(ButtonPro))]
public class ButtonProEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
    }
}
#endif
