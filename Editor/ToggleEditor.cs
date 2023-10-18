using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace CommonBase.Editor
{
    [CustomEditor(typeof(CommonBase.Toggle))]
    public class ToggleEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
        }
    }
}

