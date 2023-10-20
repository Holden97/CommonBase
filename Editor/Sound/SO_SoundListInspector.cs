using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace CommonBase.Editor
{
    [CustomEditor(typeof(SO_SoundList))]
    public class SO_SoundListInspector : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

        }
    }

}
