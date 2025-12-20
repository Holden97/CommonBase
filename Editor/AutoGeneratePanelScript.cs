#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace CommonBase
{
    [InitializeOnLoad]
    public class AutoGeneratePanelScript : UnityEditor.Editor
    {
        private const string PendingScriptKey = "ScriptCreator_PendingScriptPath";
        private const string PendingGOInstanceIDKey = "ScriptCreator_PendingGOInstanceID";

        // 用于记录已使用的变量名及其出现次数
        private static readonly Dictionary<string, int> UsedFieldNames = new();

        // 用于记录已定义的字段名
        private static readonly Dictionary<string, UIComponentInfo> FieldInfo = new();

        static AutoGeneratePanelScript()
        {
            // 始终监听 update
            EditorApplication.update += OnEditorUpdate;
        }

        private static void OnEditorUpdate()
        {
            if (EditorApplication.isCompiling) return;
            var path = SessionState.GetString(PendingScriptKey, "");
            var instanceID = SessionState.GetInt(PendingGOInstanceIDKey, 0);

            if (!string.IsNullOrEmpty(path) && instanceID != 0)
            {
                var targetGO = EditorUtility.InstanceIDToObject(instanceID) as GameObject;
                var monoScript = AssetDatabase.LoadAssetAtPath<MonoScript>(path);

                if (monoScript != null)
                {
                    var scriptType = monoScript.GetClass();
                    if (scriptType != null && scriptType.IsSubclassOf(typeof(MonoBehaviour)))
                    {
                        //UI脚本挂一次就OK
                        if (targetGO != null && targetGO.GetComponent(scriptType) == null)
                        {
                            Undo.AddComponent(targetGO, scriptType);
                            Debug.Log($"✅ 脚本 {scriptType.Name} 已挂载到 {targetGO.name}");
                        }
                    }
                    else
                    {
                        Debug.LogWarning("⚠ 脚本已创建但未能正确识别类型。可能 Unity 还未完全识别该类。");
                    }
                }

                // 清理状态
                SessionState.EraseString(PendingScriptKey);
                SessionState.EraseInt(PendingGOInstanceIDKey);
            }
        }

        [MenuItem("GameObject/自动生成Panel脚本", false, 10)]
        private static void GeneratePanelScripts(MenuCommand menuCommand)
        {
            // 优先使用 menuCommand.context，若为 null 则使用 Selection.activeGameObject
            var selectedObject = menuCommand.context as GameObject ?? Selection.activeGameObject;
            if (selectedObject == null || !IsChildOfCanvas(selectedObject))
            {
                Debug.LogError("所选对象必须是带有Canvas组件的物体的子物体");
                return;
            }

            var objectName = selectedObject.name;
            var generatedBaseFileName = $"{objectName}Gen";
            var customScriptName = objectName;

            // 创建与 panel 名称一致的文件夹
            var scriptsFolder = "Assets/Scripts/UIPanel";
            var panelSpecificFolder = Path.Combine(scriptsFolder, objectName);
            if (!Directory.Exists(panelSpecificFolder)) Directory.CreateDirectory(panelSpecificFolder);

            // 生成基类脚本
            var generateBaseScriptContent =
                GenerateAutoScriptContent(customScriptName, objectName, selectedObject);
            var baseScriptPath = Path.Combine(panelSpecificFolder, $"{generatedBaseFileName}.cs");
            File.WriteAllText(baseScriptPath, generateBaseScriptContent);

            // 生成自定义脚本
            var derivedScriptContent = GenerateCustomScriptContent(customScriptName, generatedBaseFileName);
            var derivedScriptPath = Path.Combine(panelSpecificFolder, $"{customScriptName}.cs");
            if (!File.Exists(derivedScriptPath)) File.WriteAllText(derivedScriptPath, derivedScriptContent);

            AssetDatabase.Refresh();

            // 使用 SessionState 持久化信息
            SessionState.SetString(PendingScriptKey, derivedScriptPath);
            SessionState.SetInt(PendingGOInstanceIDKey, selectedObject.GetInstanceID());
        }

        private static bool IsChildOfCanvas(GameObject obj)
        {
            var parent = obj.transform.parent;
            while (parent != null)
            {
                if (parent.GetComponent<Canvas>() != null) return true;

                parent = parent.parent;
            }

            return false;
        }

        private static string GetFieldName(string objectName)
        {
            // 使用正则表达式替换不符合命名规则的字符为下划线，并修剪空格
            var baseName = Regex.Replace(objectName.Trim(), @"[^\w]", "_");

            if (UsedFieldNames.ContainsKey(baseName))
            {
                var count = UsedFieldNames[baseName] + 1;
                UsedFieldNames[baseName] = count;
                return $"{baseName}_{count}";
            }

            UsedFieldNames[baseName] = 1;
            return baseName;
        }

        private static string GetMethodName(string objectName)
        {
            return char.ToUpperInvariant(objectName[0]) + objectName.Substring(1);
        }

        private static string GenerateAutoScriptContent(string scriptName, string objectName, GameObject rootObject)
        {
            // 每次生成脚本前清空已使用的变量名记录
            UsedFieldNames.Clear();

            var sb = new StringBuilder();
            sb.AppendLine("using UnityEngine;");
            sb.AppendLine("using UnityEngine.UI;");
            sb.AppendLine("using TMPro;");
            sb.AppendLine("using CommonBase;");
            sb.AppendLine();
            sb.AppendLine($"namespace {GetUISettingNamespace()}");
            sb.AppendLine("{");
            sb.AppendLine($"    public partial class {scriptName} : BaseUI");
            sb.AppendLine("    {");

            // 查找并添加UI控件字段
            FindAndAddUIFields(sb, rootObject.transform);

            sb.AppendLine();
            sb.AppendLine("        private void Reset()");
            sb.AppendLine("        {");
            // 赋值UI控件
            AssignUIFields(sb, rootObject.transform);
            sb.AppendLine("        }");

            sb.AppendLine("    }");
            sb.AppendLine("}");

            return sb.ToString();
        }

        private static void FindAndAddUIFields(StringBuilder sb, Transform root)
        {
            FieldInfo.Clear();
            // 遍历所有子节点，包括根节点自身
            foreach (var child in root.GetComponentsInChildren<Transform>(true))
            {
                var path = child == root ? null : GetPathFromRoot(root, child);
                var button = child.GetComponent<Button>();
                if (button != null)
                {
                    var fieldName = $"Btn{GetFieldName(child.name)}";
                    sb.AppendLine($"        public Button {fieldName};");
                    // sb.AppendLine($"        public void On{fieldName}Clicked() {{ }}");
                    FieldInfo.Add(fieldName, new UIComponentInfo(button, path, fieldName));
                }
                else if (child.TryGetComponent<Image>(out var img))
                {
                    var fieldName = $"Img{GetFieldName(child.name)}";
                    sb.AppendLine($"        public Image {fieldName};");
                    FieldInfo.Add(fieldName, new UIComponentInfo(img, path, fieldName));
                }
                else if (child.TryGetComponent<TextMeshProUGUI>(out var tmp))
                {
                    var fieldName = $"TMP{GetFieldName(child.name)}";
                    sb.AppendLine($"        public TextMeshProUGUI {fieldName};");
                    FieldInfo.Add(fieldName, new UIComponentInfo(tmp, path, fieldName));
                }
                else if (child.TryGetComponent<ScrollRect>(out var rect))
                {
                    var fieldName = $"ScrollRect{GetFieldName(child.name)}";
                    sb.AppendLine($"        public ScrollRect {fieldName};");
                    FieldInfo.Add(fieldName, new UIComponentInfo(rect, path, fieldName));
                }
                // 可根据需要添加更多UI控件类型
            }
        }

        private static void AssignUIFields(StringBuilder sb, Transform root)
        {
            foreach (var keyValuePair in FieldInfo)
            {
                var fieldName = keyValuePair.Key;
                // 只处理已定义的字段
                var info = keyValuePair.Value;
                var child = keyValuePair.Value.component;
                if (child != null)
                {
                    if (child.GetComponent<Button>() != null)
                    {
                        sb.AppendLine(info.path.IsNullOrEmpty()
                            ? $"            {fieldName} = transform.GetComponent<Button>();"
                            : $"            {fieldName} = transform.Find(\"{info.path}\").GetComponent<Button>();"
                        );
                        sb.AppendLine(
                            $"            {fieldName}.onClick.AddListener(On{GetMethodName(fieldName)}Clicked);");
                    }
                    else if (child.GetComponent<Image>() != null)
                    {
                        sb.AppendLine(info.path.IsNullOrEmpty()
                            ? $"            {fieldName} = transform.GetComponent<Image>();"
                            : $"            {fieldName} = transform.Find(\"{info.path}\").GetComponent<Image>();"
                        );
                    }
                    else if (child.GetComponent<TextMeshProUGUI>() != null)
                    {
                        sb.AppendLine(info.path.IsNullOrEmpty()
                            ? $"            {fieldName} = transform.GetComponent<TextMeshProUGUI>();"
                            : $"            {fieldName} = transform.Find(\"{info.path}\").GetComponent<TextMeshProUGUI>();"
                        );
                    }
                }
            }
        }

        private static string GetPathFromRoot(Transform root, Transform child)
        {
            var path = child.name;
            var current = child.parent;
            while (current != root && current != null)
            {
                path = current.name + "/" + path;
                current = current.parent;
            }

            return path;
        }

        public static string GetUISettingNamespace()
        {
            var uiPath = Resources.Load<SO_UIPath>("SO/UIPath");
            return uiPath.uiNamespace;
        }


        private static string GenerateCustomScriptContent(string customScriptName, string baseScriptName)
        {
            var sb = new StringBuilder();
            sb.AppendLine("using UnityEngine;");
            sb.AppendLine("using CommonBase;");
            sb.AppendLine();
            sb.AppendLine($"namespace {GetUISettingNamespace()}");
            sb.AppendLine("{");
            sb.AppendLine($"    public partial class {customScriptName} : BaseUI");
            sb.AppendLine("    {");
            foreach (var child in FieldInfo)
                if (child.Value.component is Button)
                    sb.AppendLine($"        public void On{child.Value.name}Clicked() {{ }}");

            sb.AppendLine("");
            sb.AppendLine("    }");
            sb.AppendLine("}");

            return sb.ToString();
        }

        [MenuItem("GameObject/自动生成Panel脚本", true)]
        private static bool ValidateGeneratePanelScripts(MenuCommand menuCommand)
        {
            // 优先使用 menuCommand.context，若为 null 则使用 Selection.activeGameObject
            var selectedObject = menuCommand.context as GameObject ?? Selection.activeGameObject;
            return selectedObject != null && IsChildOfCanvas(selectedObject);
        }
    }
}

#endif