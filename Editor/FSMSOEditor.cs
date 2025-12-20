#if UNITY_EDITOR
using Sirenix.OdinInspector.Editor;
using UnityEditor;
using UnityEngine;
using System.IO;
using CommonBase;

[CustomEditor(typeof(FSMSO))]
public class FSMSOEditor : OdinEditor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        if (GUILayout.Button("生成C#代码"))
        {
            var fsms = (FSMSO)target;
            foreach (var state in fsms.states)
            {
                GenerateClass(target.name, state.stateName, state.stateClass, fsms.assemblyName);
            }
        }
    }

    private void GenerateClass(string fsmName, string stateName, string className, string assemblyName)
    {
        // 确保目录存在
        string path = $"Assets/Scripts/FSM/{fsmName}";
        if (!AssetDatabase.IsValidFolder(path))
        {
            AssetDatabase.CreateFolder("Assets/Scripts/FSM", fsmName);
        }

        // 检查文件是否已经存在
        string filePath = $"{path}/{className}.cs";
        if (File.Exists(filePath))
        {
            Debug.LogWarning($"File {filePath} already exists. Skipping generation for {className}.");
            return;
        }

        // 生成C#类的内容
        string content = $@"using System;
using CommonBase;

namespace {assemblyName}
{{
    [Serializable]
    public class {className} : BaseState
    {{
        public {className}(string stateName, FiniteStateMachine fsm) : base(stateName, fsm) {{ }}

        public override void OnStateStart() {{  }}
        public override void OnStateUpdate() {{  }}
        public override void OnStateEnd() {{  }}
        public override void OnStateCheckTransition() {{  }}
        public override void OnReset() {{  }}
        public override void OnDispose() {{  }}
    }}
}}";

        // 保存到文件
        File.WriteAllText(filePath, content);
        AssetDatabase.Refresh();
    }
}

#endif