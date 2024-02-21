#if UNITY_EDITOR
#if ENABLE_DOTWEEN
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
#endif
