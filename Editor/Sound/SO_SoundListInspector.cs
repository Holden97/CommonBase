#if UNITY_EDITOR

using UnityEditor;

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

#endif