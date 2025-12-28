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
        public static void GeneratePanelScripts(MenuCommand menuCommand)
        {
            // 优先使用 menuCommand.context，若为 null 则使用 Selection.activeGameObject
            var selectedObject = menuCommand.context as GameObject ?? Selection.activeGameObject;
            if (selectedObject == null)
            {
                Debug.LogError("请选择一个GameObject");
                return;
            }

            // 检查是否有 ReferenceCollector 组件
            var referenceCollector = selectedObject.GetComponent<ReferenceCollector>();
            UIGenerationType generationType = UIGenerationType.Panel;

            if (referenceCollector != null)
            {
                generationType = referenceCollector.generationType;
            }

            // 根据类型生成不同的脚本
            if (generationType == UIGenerationType.Widget)
            {
                GenerateWidgetScript(selectedObject, referenceCollector);
            }
            else if (generationType == UIGenerationType.FloatWindow)
            {
                GenerateFloatWindowScript(selectedObject, referenceCollector);
            }
            else
            {
                GeneratePanelScript(selectedObject, referenceCollector);
            }
        }

        /// <summary>
        /// 生成 Panel 类型的脚本
        /// </summary>
        private static void GeneratePanelScript(GameObject selectedObject, ReferenceCollector referenceCollector)
        {
            if (!IsChildOfCanvas(selectedObject))
            {
                Debug.LogError("Panel 类型的对象必须是带有Canvas组件的物体的子物体");
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
                GenerateAutoScriptContent(customScriptName, objectName, selectedObject, UIGenerationType.Panel);
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

        /// <summary>
        /// 生成 Widget 类型的脚本
        /// </summary>
        private static void GenerateWidgetScript(GameObject selectedObject, ReferenceCollector referenceCollector)
        {
            var objectName = selectedObject.name;
            var scriptName = objectName;

            // 创建 Widget 文件夹
            var scriptsFolder = "Assets/Scripts/UIWidget";
            if (!Directory.Exists(scriptsFolder)) Directory.CreateDirectory(scriptsFolder);

            // 生成 .Designer.cs 文件（组件引用部分 - 会被覆盖）
            var designerContent = GenerateWidgetDesignerScriptContent(scriptName, selectedObject);
            var designerPath = Path.Combine(scriptsFolder, $"{scriptName}.Designer.cs");
            File.WriteAllText(designerPath, designerContent);

            // 生成 .cs 文件（业务逻辑部分 - 不会被覆盖）
            var scriptContent = GenerateWidgetScriptContent(scriptName, selectedObject);
            var scriptPath = Path.Combine(scriptsFolder, $"{scriptName}.cs");

            // 只有当业务逻辑文件不存在时才创建
            if (!File.Exists(scriptPath))
            {
                File.WriteAllText(scriptPath, scriptContent);
            }

            AssetDatabase.Refresh();

            // 直接挂载到当前对象上
            SessionState.SetString(PendingScriptKey, scriptPath);
            SessionState.SetInt(PendingGOInstanceIDKey, selectedObject.GetInstanceID());

            Debug.Log($"✅ 已生成 Widget 脚本: {scriptName}");
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

        private static string GenerateAutoScriptContent(string scriptName, string objectName, GameObject rootObject, UIGenerationType generationType)
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

            // 检查是否有 ReferenceCollector 组件
            var referenceCollector = rootObject.GetComponent<ReferenceCollector>();
            var useReferenceCollector = referenceCollector != null && referenceCollector.data.Count > 0;

            // 如果使用 ReferenceCollector，添加字段
            if (useReferenceCollector)
            {
                sb.AppendLine("        private ReferenceCollector referenceCollector;");
            }

            // 查找并添加UI控件字段
            FindAndAddUIFields(sb, rootObject.transform, referenceCollector);

            sb.AppendLine();
            sb.AppendLine("        private void Reset()");
            sb.AppendLine("        {");
            // 赋值UI控件
            AssignUIFields(sb, rootObject.transform, referenceCollector);
            sb.AppendLine("        }");

            sb.AppendLine("    }");
            sb.AppendLine("}");

            return sb.ToString();
        }

        /// <summary>
        /// 生成 Widget Designer 脚本内容（组件引用部分 - 可自动覆盖）
        /// </summary>
        private static string GenerateWidgetDesignerScriptContent(string scriptName, GameObject rootObject)
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
            sb.AppendLine("    /// <summary>");
            sb.AppendLine("    /// 组件引用部分 - 由工具自动生成，请勿手动修改");
            sb.AppendLine("    /// </summary>");
            sb.AppendLine($"    public partial class {scriptName}");
            sb.AppendLine("    {");

            // 检查是否有 ReferenceCollector 组件
            var referenceCollector = rootObject.GetComponent<ReferenceCollector>();
            var useReferenceCollector = referenceCollector != null && referenceCollector.data.Count > 0;

            // 如果使用 ReferenceCollector，添加字段
            if (useReferenceCollector)
            {
                sb.AppendLine("        private ReferenceCollector referenceCollector;");
                sb.AppendLine();
            }

            // 查找并添加UI控件字段
            FindAndAddUIFields(sb, rootObject.transform, referenceCollector);

            sb.AppendLine();
            sb.AppendLine("        private void Reset()");
            sb.AppendLine("        {");
            // 赋值UI控件
            AssignUIFields(sb, rootObject.transform, referenceCollector);
            sb.AppendLine("        }");

            sb.AppendLine();
            sb.AppendLine("        private void Awake()");
            sb.AppendLine("        {");
            // 赋值UI控件
            AssignUIFields(sb, rootObject.transform, referenceCollector);
            sb.AppendLine("        }");

            sb.AppendLine("    }");
            sb.AppendLine("}");

            return sb.ToString();
        }

        /// <summary>
        /// 生成 Widget 脚本内容（业务逻辑部分 - 可自由修改）
        /// </summary>
        private static string GenerateWidgetScriptContent(string scriptName, GameObject rootObject)
        {
            // 需要先调用FindAndAddUIFields来填充FieldInfo，以便生成按钮事件
            UsedFieldNames.Clear();
            var tempSb = new StringBuilder();
            var referenceCollector = rootObject.GetComponent<ReferenceCollector>();
            FindAndAddUIFields(tempSb, rootObject.transform, referenceCollector);

            var sb = new StringBuilder();
            sb.AppendLine("using UnityEngine;");
            sb.AppendLine("using CommonBase;");
            sb.AppendLine();
            sb.AppendLine($"namespace {GetUISettingNamespace()}");
            sb.AppendLine("{");
            sb.AppendLine("    /// <summary>");
            sb.AppendLine("    /// 业务逻辑部分 - 可自由修改");
            sb.AppendLine("    /// </summary>");
            sb.AppendLine($"    public partial class {scriptName} : MonoBehaviour, IUIWidget");
            sb.AppendLine("    {");

            sb.AppendLine("        public void Initialize()");
            sb.AppendLine("        {");
            sb.AppendLine("            // TODO: 初始化 Widget");
            sb.AppendLine("        }");

            sb.AppendLine();
            sb.AppendLine("        public void Show()");
            sb.AppendLine("        {");
            sb.AppendLine("            gameObject.SetActive(true);");
            sb.AppendLine("        }");

            sb.AppendLine();
            sb.AppendLine("        public void Hide()");
            sb.AppendLine("        {");
            sb.AppendLine("            gameObject.SetActive(false);");
            sb.AppendLine("        }");

            // 为按钮生成事件方法
            foreach (var child in FieldInfo)
            {
                if (child.Value.component is Button)
                {
                    sb.AppendLine();
                    sb.AppendLine($"        private void On{child.Value.name}Clicked()");
                    sb.AppendLine("        {");
                    sb.AppendLine("            // TODO: 处理按钮点击");
                    sb.AppendLine("        }");
                }
            }

            sb.AppendLine("    }");
            sb.AppendLine("}");

            return sb.ToString();
        }

        private static void FindAndAddUIFields(StringBuilder sb, Transform root, ReferenceCollector referenceCollector)
        {
            FieldInfo.Clear();

            // 如果有 ReferenceCollector，则从中读取引用
            if (referenceCollector != null && referenceCollector.data.Count > 0)
            {
                foreach (var refData in referenceCollector.data)
                {
                    if (refData.gameObject == null) continue;

                    var fieldName = GetFieldName(refData.key);
                    UnityEngine.Object targetObj = refData.gameObject;
                    string typeString = "";

                    // 检测引用的类型
                    if (targetObj is GameObject go)
                    {
                        typeString = "GameObject";
                    }
                    else if (targetObj is Component component)
                    {
                        typeString = component.GetType().Name;
                    }
                    else
                    {
                        Debug.LogWarning($"未知的对象类型: {targetObj.GetType().Name}");
                        continue;
                    }

                    // 生成字段
                    sb.AppendLine($"        public {typeString} {fieldName};");
                    FieldInfo.Add(fieldName, new UIComponentInfo(targetObj, refData.key, fieldName, typeString));
                }
            }
            else
            {
                // 使用原来的逻辑：遍历所有子节点，包括根节点自身
                foreach (var child in root.GetComponentsInChildren<Transform>(true))
                {
                    var path = child == root ? null : GetPathFromRoot(root, child);
                    var button = child.GetComponent<Button>();
                    if (button != null)
                    {
                        var fieldName = $"Btn{GetFieldName(child.name)}";
                        sb.AppendLine($"        public Button {fieldName};");
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
        }

        private static void AssignUIFields(StringBuilder sb, Transform root, ReferenceCollector referenceCollector)
        {
            // 如果使用 ReferenceCollector，先初始化它
            if (referenceCollector != null && referenceCollector.data.Count > 0)
            {
                sb.AppendLine("            referenceCollector = GetComponent<ReferenceCollector>();");
                sb.AppendLine();
            }

            foreach (var keyValuePair in FieldInfo)
            {
                var fieldName = keyValuePair.Key;
                var info = keyValuePair.Value;
                var child = keyValuePair.Value.component;

                // 判断是否使用 ReferenceCollector
                bool useReferenceCollector = referenceCollector != null && referenceCollector.data.Count > 0;

                // 获取类型字符串
                string typeString = string.IsNullOrEmpty(info.typeString) ? GetComponentTypeName(child) : info.typeString;

                if (typeString == "GameObject")
                {
                    // GameObject 类型特殊处理
                    if (useReferenceCollector)
                    {
                        sb.AppendLine($"            {fieldName} = referenceCollector.Get<GameObject>(\"{info.path}\");");
                    }
                    else
                    {
                        sb.AppendLine(info.path.IsNullOrEmpty()
                            ? $"            {fieldName} = gameObject;"
                            : $"            {fieldName} = transform.Find(\"{info.path}\").gameObject;"
                        );
                    }
                }
                else
                {
                    // 其他组件类型
                    if (useReferenceCollector)
                    {
                        sb.AppendLine($"            {fieldName} = referenceCollector.Get<{typeString}>(\"{info.path}\");");
                    }
                    else
                    {
                        sb.AppendLine(info.path.IsNullOrEmpty()
                            ? $"            {fieldName} = transform.GetComponent<{typeString}>();"
                            : $"            {fieldName} = transform.Find(\"{info.path}\").GetComponent<{typeString}>();"
                        );
                    }

                    // 如果是 Button，添加事件监听
                    if (typeString == "Button")
                    {
                        sb.AppendLine($"            {fieldName}.onClick.AddListener(On{GetMethodName(fieldName)}Clicked);");
                    }
                }
            }
        }

        /// <summary>
        /// 获取组件的类型名称
        /// </summary>
        private static string GetComponentTypeName(Component component)
        {
            if (component == null) return "Transform";
            if (component is Button) return "Button";
            if (component is Image) return "Image";
            if (component is TextMeshProUGUI) return "TextMeshProUGUI";
            if (component is ScrollRect) return "ScrollRect";
            if (component is Text) return "Text";
            return "Transform";
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

        /// <summary>
        /// 生成 FloatWindow 类型的脚本
        /// </summary>
        private static void GenerateFloatWindowScript(GameObject selectedObject, ReferenceCollector referenceCollector)
        {
            var objectName = selectedObject.name;
            var scriptName = objectName;

            // 创建 FloatWindow 文件夹
            var scriptsFolder = "Assets/Scripts/FloatWindow";
            if (!Directory.Exists(scriptsFolder)) Directory.CreateDirectory(scriptsFolder);

            // 生成 .Designer.cs 文件（组件引用部分 - 会被覆盖）
            var designerContent = GenerateFloatWindowDesignerScriptContent(scriptName, selectedObject);
            var designerPath = Path.Combine(scriptsFolder, $"{scriptName}.Designer.cs");
            File.WriteAllText(designerPath, designerContent);

            // 生成 .cs 文件（业务逻辑部分 - 不会被覆盖）
            var scriptContent = GenerateFloatWindowScriptContent(scriptName, selectedObject);
            var scriptPath = Path.Combine(scriptsFolder, $"{scriptName}.cs");

            // 只有当业务逻辑文件不存在时才创建
            if (!File.Exists(scriptPath))
            {
                File.WriteAllText(scriptPath, scriptContent);
            }

            AssetDatabase.Refresh();

            // 直接挂载到当前对象上
            SessionState.SetString(PendingScriptKey, scriptPath);
            SessionState.SetInt(PendingGOInstanceIDKey, selectedObject.GetInstanceID());

            Debug.Log($"✅ 已生成 FloatWindow 脚本: {scriptName}");
        }

        /// <summary>
        /// 生成 FloatWindow Designer 脚本内容（组件引用部分 - 可自动覆盖）
        /// </summary>
        private static string GenerateFloatWindowDesignerScriptContent(string scriptName, GameObject rootObject)
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
            sb.AppendLine("    /// <summary>");
            sb.AppendLine("    /// 组件引用部分 - 由工具自动生成，请勿手动修改");
            sb.AppendLine("    /// </summary>");
            sb.AppendLine($"    public partial class {scriptName}");
            sb.AppendLine("    {");

            // 检查是否有 ReferenceCollector 组件
            var referenceCollector = rootObject.GetComponent<ReferenceCollector>();
            var useReferenceCollector = referenceCollector != null && referenceCollector.data.Count > 0;

            // 如果使用 ReferenceCollector，添加字段
            if (useReferenceCollector)
            {
                sb.AppendLine("        private ReferenceCollector referenceCollector;");
                sb.AppendLine();
            }

            // 查找并添加UI控件字段
            FindAndAddUIFields(sb, rootObject.transform, referenceCollector);

            sb.AppendLine();
            sb.AppendLine("        private void Reset()");
            sb.AppendLine("        {");
            // 赋值UI控件
            AssignUIFields(sb, rootObject.transform, referenceCollector);
            sb.AppendLine("        }");

            sb.AppendLine("    }");
            sb.AppendLine("}");

            return sb.ToString();
        }

        /// <summary>
        /// 生成 FloatWindow 脚本内容（业务逻辑部分 - 可自由修改）
        /// </summary>
        private static string GenerateFloatWindowScriptContent(string scriptName, GameObject rootObject)
        {
            // 需要先调用FindAndAddUIFields来填充FieldInfo，以便生成按钮事件
            UsedFieldNames.Clear();
            var tempSb = new StringBuilder();
            var referenceCollector = rootObject.GetComponent<ReferenceCollector>();
            FindAndAddUIFields(tempSb, rootObject.transform, referenceCollector);

            var sb = new StringBuilder();
            sb.AppendLine("using UnityEngine;");
            sb.AppendLine("using CommonBase;");
            sb.AppendLine();
            sb.AppendLine($"namespace {GetUISettingNamespace()}");
            sb.AppendLine("{");
            sb.AppendLine("    /// <summary>");
            sb.AppendLine($"    /// {scriptName} - 浮窗");
            sb.AppendLine("    /// 业务逻辑部分 - 可自由修改");
            sb.AppendLine("    /// </summary>");
            sb.AppendLine($"    public partial class {scriptName} : BaseFloatWindow");
            sb.AppendLine("    {");

            // 重写 Awake（可选）
            sb.AppendLine("        protected override void Awake()");
            sb.AppendLine("        {");
            sb.AppendLine("            base.Awake();");
            sb.AppendLine();
            sb.AppendLine("            // 初始化组件引用");
            sb.AppendLine("            Reset();");
            sb.AppendLine();
            sb.AppendLine("            // TODO: 配置浮窗特性");
            sb.AppendLine("            // defaultOffset = new Vector3(0, 50, 0);  // 设置默认偏移");
            sb.AppendLine("            // autoHideWhenTargetInactive = true;      // 目标不活跃时自动隐藏");
            sb.AppendLine("            // updatePositionEveryFrame = true;        // 每帧更新位置");
            sb.AppendLine("        }");

            sb.AppendLine();
            sb.AppendLine("        protected override void OnShow(object data)");
            sb.AppendLine("        {");
            sb.AppendLine("            base.OnShow(data);");
            sb.AppendLine();
            sb.AppendLine("            // TODO: 浮窗显示时的逻辑");
            sb.AppendLine("            // 例如：根据 data 更新UI内容");
            sb.AppendLine("        }");

            sb.AppendLine();
            sb.AppendLine("        protected override void OnHide()");
            sb.AppendLine("        {");
            sb.AppendLine("            base.OnHide();");
            sb.AppendLine();
            sb.AppendLine("            // TODO: 浮窗隐藏时的逻辑");
            sb.AppendLine("        }");

            sb.AppendLine();
            sb.AppendLine("        protected override void OnPositionUpdated()");
            sb.AppendLine("        {");
            sb.AppendLine("            base.OnPositionUpdated();");
            sb.AppendLine();
            sb.AppendLine("            // TODO: 位置更新时的逻辑（可选）");
            sb.AppendLine("        }");

            // 为按钮生成事件方法
            foreach (var child in FieldInfo)
            {
                if (child.Value.component is Button)
                {
                    sb.AppendLine();
                    sb.AppendLine($"        private void On{child.Value.name}Clicked()");
                    sb.AppendLine("        {");
                    sb.AppendLine("            // TODO: 处理按钮点击");
                    sb.AppendLine("        }");
                }
            }

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