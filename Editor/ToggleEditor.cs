using UnityEditor;

namespace CommonBase.Editor
{
    [CustomEditor(typeof(Toggle), true)]
    public class ToggleEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
        }
    }
}

