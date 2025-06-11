using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Text.RegularExpressions;
using System.Collections.Generic;

namespace CommonBase
{
    [InitializeOnLoad]
    public class AutoGeneratePanelScript : UnityEditor.Editor
    {

        const string PendingScriptKey = "ScriptCreator_PendingScriptPath";
        const string PendingGOInstanceIDKey = "ScriptCreator_PendingGOInstanceID";

        static AutoGeneratePanelScript()
        {
            // 始终监听 update
            EditorApplication.update += OnEditorUpdate;
        }
        // 用于记录已使用的变量名及其出现次数
        private static Dictionary<string, int> usedFieldNames = new Dictionary<string, int>();
        // 用于记录已定义的字段名
        private static Dictionary<string, string> definedFieldNames = new Dictionary<string, string>();

        static void OnEditorUpdate()
        {
            if (EditorApplication.isCompiling) return;
            string path = SessionState.GetString(PendingScriptKey, "");
            int instanceID = SessionState.GetInt(PendingGOInstanceIDKey, 0);

            if (!string.IsNullOrEmpty(path) && instanceID != 0)
            {
                GameObject targetGO = EditorUtility.InstanceIDToObject(instanceID) as GameObject;
                MonoScript monoScript = AssetDatabase.LoadAssetAtPath<MonoScript>(path);

                if (monoScript != null)
                {
                    System.Type scriptType = monoScript.GetClass();
                    if (scriptType != null && scriptType.IsSubclassOf(typeof(MonoBehaviour)))
                    {
                        if (targetGO != null)
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
            GameObject selectedObject = menuCommand.context as GameObject ?? Selection.activeGameObject;
            if (selectedObject == null || !IsChildOfCanvas(selectedObject))
            {
                Debug.LogError("所选对象必须是带有Canvas组件的物体的子物体");
                return;
            }

            string objectName = selectedObject.name;
            string baseScriptName = $"{objectName}Base";
            string derivedScriptName = objectName;

            // 创建与 panel 名称一致的文件夹
            string scriptsFolder = "Assets/Scripts/UIPanel";
            string panelSpecificFolder = Path.Combine(scriptsFolder, objectName);
            if (!Directory.Exists(panelSpecificFolder))
            {
                Directory.CreateDirectory(panelSpecificFolder);
            }

            // 生成基类脚本
            string baseScriptContent = GenerateBaseScriptContent(baseScriptName, objectName, selectedObject);
            string baseScriptPath = Path.Combine(panelSpecificFolder, $"{baseScriptName}.cs");
            File.WriteAllText(baseScriptPath, baseScriptContent);

            // 生成派生类脚本
            string derivedScriptContent = GenerateDerivedScriptContent(derivedScriptName, baseScriptName);
            string derivedScriptPath = Path.Combine(panelSpecificFolder, $"{derivedScriptName}.cs");
            File.WriteAllText(derivedScriptPath, derivedScriptContent);

            AssetDatabase.Refresh();

            // 使用 SessionState 持久化信息
            SessionState.SetString(PendingScriptKey, derivedScriptPath);
            SessionState.SetInt(PendingGOInstanceIDKey, selectedObject.GetInstanceID());
        }

        private static bool IsChildOfCanvas(GameObject obj)
        {
            Transform parent = obj.transform.parent;
            while (parent != null)
            {
                if (parent.GetComponent<Canvas>() != null)
                {
                    return true;
                }
                parent = parent.parent;
            }
            return false;
        }

        private static string GetFieldName(string objectName)
        {
            // 使用正则表达式替换不符合命名规则的字符为下划线，并修剪空格
            string baseName = Regex.Replace(objectName.Trim(), @"[^\w]", "_");

            if (usedFieldNames.ContainsKey(baseName))
            {
                int count = usedFieldNames[baseName] + 1;
                usedFieldNames[baseName] = count;
                return $"{baseName}_{count}";
            }
            else
            {
                usedFieldNames[baseName] = 1;
                return baseName;
            }
        }

        private static string GetMethodName(string objectName)
        {
            return char.ToUpperInvariant(objectName[0]) + objectName.Substring(1);
        }

        private static string GenerateBaseScriptContent(string baseScriptName, string objectName, GameObject rootObject)
        {
            // 每次生成脚本前清空已使用的变量名记录
            usedFieldNames.Clear();

            StringBuilder sb = new StringBuilder();
            sb.AppendLine("using UnityEngine;");
            sb.AppendLine("using UnityEngine.UI;");
            sb.AppendLine("using TMPro;");
            sb.AppendLine("using CommonBase;");
            sb.AppendLine();
            sb.AppendLine($"namespace Traingeon");
            sb.AppendLine("{");
            sb.AppendLine($"    public class {baseScriptName} : BaseUI");
            sb.AppendLine("    {");

            // 查找并添加UI控件字段
            FindAndAddUIFields(sb, rootObject.transform);

            sb.AppendLine();
            // 去掉 override 关键字
            sb.AppendLine("        public void Start()");
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
            definedFieldNames.Clear();
            foreach (Transform child in root.GetComponentsInChildren<Transform>(true))
            {
                if (child == root) continue;
                var path = GetPathFromRoot(root, child);

                Button button = child.GetComponent<Button>();
                if (button != null)
                {
                    string fieldName = GetFieldName(child.name);
                    sb.AppendLine($"        public Button {fieldName};");
                    sb.AppendLine($"        public virtual void On{GetMethodName(fieldName)}Clicked() {{ }}");
                    definedFieldNames.Add(fieldName, path);
                }
                else if (child.GetComponent<Image>() != null)
                {
                    string fieldName = GetFieldName(child.name);
                    sb.AppendLine($"        public Image {fieldName};");
                    definedFieldNames.Add(fieldName, path);
                }
                else if (child.GetComponent<TextMeshProUGUI>() != null)
                {
                    string fieldName = GetFieldName(child.name);
                    sb.AppendLine($"        public TextMeshProUGUI {fieldName};");
                    definedFieldNames.Add(fieldName, path);
                }
                // 可根据需要添加更多UI控件类型
            }
        }

        private static void AssignUIFields(StringBuilder sb, Transform root)
        {
            foreach (var pair in definedFieldNames)
            {
                string fieldName = pair.Key;
                // 只处理已定义的字段
                string path = pair.Value;
                Transform child = root.Find(path);
                if (child != null)
                {
                    if (child.GetComponent<Button>() != null)
                    {
                        sb.AppendLine($"            {fieldName} = transform.Find(\"{path}\").GetComponent<Button>();");
                        sb.AppendLine($"            {fieldName}.onClick.AddListener(On{GetMethodName(fieldName)}Clicked);");
                    }
                    else if (child.GetComponent<Image>() != null)
                    {
                        sb.AppendLine($"            {fieldName} = transform.Find(\"{path}\").GetComponent<Image>();");
                    }
                    else if (child.GetComponent<TextMeshProUGUI>() != null)
                    {
                        sb.AppendLine($"            {fieldName} = transform.Find(\"{path}\").GetComponent<TextMeshProUGUI>();");
                    }
                }
            }
        }

        private static string GetPathFromRoot(Transform root, Transform child)
        {
            string path = child.name;
            Transform current = child.parent;
            while (current != root && current != null)
            {
                path = current.name + "/" + path;
                current = current.parent;
            }
            return path;
        }

        private static string GenerateDerivedScriptContent(string derivedScriptName, string baseScriptName)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("using UnityEngine;");
            sb.AppendLine("using CommonBase;");
            sb.AppendLine();
            sb.AppendLine($"namespace Traingeon");
            sb.AppendLine("{");
            sb.AppendLine($"    public class {derivedScriptName} : {baseScriptName}");
            sb.AppendLine("    {");
            // sb.AppendLine("        // 可在此处重写基类方法");
            sb.AppendLine("");
            sb.AppendLine("    }");
            sb.AppendLine("}");

            return sb.ToString();
        }

        [MenuItem("GameObject/自动生成Panel脚本", true)]
        private static bool ValidateGeneratePanelScripts(MenuCommand menuCommand)
        {
            // 优先使用 menuCommand.context，若为 null 则使用 Selection.activeGameObject
            GameObject selectedObject = menuCommand.context as GameObject ?? Selection.activeGameObject;
            return selectedObject != null && IsChildOfCanvas(selectedObject);
        }
    }
}
