#if UNITY_EDITOR
using System.Collections.Generic;
using System.Text.RegularExpressions;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace CommonBase
{
    /// <summary>
    /// ReferenceCollector 的自定义编辑器，提供可视化的引用管理界面
    /// </summary>
    [CustomEditor(typeof(ReferenceCollector))]
    public class ReferenceCollectorEditor : UnityEditor.Editor
    {
        private string searchKey
        {
            get { return _searchKey; }
            set
            {
                if (_searchKey != value)
                {
                    _searchKey = value;
                    targetObject = referenceCollector.Get<UnityEngine.Object>(searchKey);
                }
            }
        }

        private ReferenceCollector referenceCollector;
        private UnityEngine.Object targetObject;
        private string _searchKey = "";


        private SerializedProperty generationTypeProperty;

        /// <summary>
        /// 删除所有空引用
        /// </summary>
        private void DelNullReference()
        {
            var dataProperty = serializedObject.FindProperty("data");
            for (int i = dataProperty.arraySize - 1; i >= 0; i--)
            {
                var gameObjectProperty = dataProperty.GetArrayElementAtIndex(i).FindPropertyRelative("gameObject");
                if (gameObjectProperty.objectReferenceValue == null)
                {
                    dataProperty.DeleteArrayElementAtIndex(i);
                    EditorUtility.SetDirty(referenceCollector);
                    serializedObject.ApplyModifiedProperties();
                    serializedObject.UpdateIfRequiredOrScript();
                }
            }
        }

        private void OnEnable()
        {
            referenceCollector = (ReferenceCollector)target;
            generationTypeProperty = serializedObject.FindProperty("generationType");
        }

        public override void OnInspectorGUI()
        {
            Undo.RecordObject(referenceCollector, "Changed Settings");

            var dataProperty = serializedObject.FindProperty("data");

            // 生成类型选择
            EditorGUILayout.PropertyField(generationTypeProperty, new GUIContent("生成类型", "Panel: 完整的UI面板（继承BaseUI）\nWidget: UI小部件（实现IUIWidget接口）\nFloatWindow: 浮窗（继承BaseFloatWindow并实现IFloatWindow接口）"));
            serializedObject.ApplyModifiedProperties();

            EditorGUILayout.Space();

            // 第一行：生成脚本按钮
            GUILayout.BeginHorizontal();

            // 使用绿色背景突出显示生成脚本按钮
            var originalColor = GUI.backgroundColor;
            GUI.backgroundColor = new Color(0.5f, 1f, 0.5f);
            if (GUILayout.Button("生成 UI 脚本", GUILayout.Height(30)))
            {
                GenerateUIScript();
            }
            GUI.backgroundColor = originalColor;

            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space();

            // 功能按钮行
            GUILayout.BeginHorizontal();

            if (GUILayout.Button("Clear All"))
            {
                referenceCollector.Clear();
            }

            if (GUILayout.Button("Delete Null References"))
            {
                DelNullReference();
            }

            if (GUILayout.Button("Sort"))
            {
                referenceCollector.Sort();
            }

            EditorGUILayout.EndHorizontal();

            // 第二行：重命名按钮
            GUILayout.BeginHorizontal();

            if (GUILayout.Button("Fix All Key Names"))
            {
                FixAllKeyNames();
            }

            EditorGUILayout.EndHorizontal();

            // 搜索和删除行
            EditorGUILayout.BeginHorizontal();

            EditorGUILayout.LabelField("Search:", GUILayout.Width(50));
            searchKey = EditorGUILayout.TextField(searchKey);
            EditorGUILayout.ObjectField(targetObject, typeof(UnityEngine.Object), false);

            if (GUILayout.Button("Delete"))
            {
                referenceCollector.Remove(searchKey);
                targetObject = null;
            }

            GUILayout.EndHorizontal();
            EditorGUILayout.Space();

            var delList = new List<int>();
            SerializedProperty property;

            // 显示所有引用
            for (int i = referenceCollector.data.Count - 1; i >= 0; i--)
            {
                GUILayout.BeginHorizontal();

                // 显示key和对应的对象
                property = dataProperty.GetArrayElementAtIndex(i).FindPropertyRelative("key");
                string newKey = EditorGUILayout.DelayedTextField(property.stringValue, GUILayout.Width(120));
                if (newKey != property.stringValue)
                {
                    if (referenceCollector.data.Exists(x => x.key == newKey))
                    {
                        Debug.LogError($"Key '{newKey}' already exists!");
                    }
                    else
                    {
                        property.stringValue = newKey;
                    }
                }

                // 获取当前引用的对象
                var objProperty = dataProperty.GetArrayElementAtIndex(i).FindPropertyRelative("gameObject");
                var currentObj = objProperty.objectReferenceValue;

                // 显示动态组件类型下拉菜单
                if (currentObj != null)
                {
                    DrawComponentTypeDropdown(objProperty, property, currentObj, i);
                }
                else
                {
                    EditorGUILayout.LabelField("(无引用)", GUILayout.Width(120));
                }

                // 显示对象字段
                objProperty.objectReferenceValue =
                    EditorGUILayout.ObjectField(objProperty.objectReferenceValue, typeof(UnityEngine.Object), true);

                // 删除按钮
                if (GUILayout.Button("X", GUILayout.Width(25)))
                {
                    delList.Add(i);
                }

                GUILayout.EndHorizontal();
            }

            // 拖拽区域提示
            EditorGUILayout.Space();
            EditorGUILayout.HelpBox("拖拽 UI 组件到此处自动添加引用（支持识别 Button、Image、TextMeshProUGUI、ScrollRect 等）", MessageType.Info);

            var eventType = Event.current.type;

            // 支持拖拽添加
            if (eventType == EventType.DragUpdated || eventType == EventType.DragPerform)
            {
                DragAndDrop.visualMode = DragAndDropVisualMode.Copy;

                if (eventType == EventType.DragPerform)
                {
                    DragAndDrop.AcceptDrag();
                    foreach (var o in DragAndDrop.objectReferences)
                    {
                        AddReferenceWithAutoName(dataProperty, o);
                    }
                }

                Event.current.Use();
            }

            // 删除标记的元素
            foreach (var i in delList)
            {
                dataProperty.DeleteArrayElementAtIndex(i);
            }

            serializedObject.ApplyModifiedProperties();
            serializedObject.UpdateIfRequiredOrScript();
        }

        /// <summary>
        /// 添加新的引用（带自动识别组件类型和命名）
        /// </summary>
        private void AddReferenceWithAutoName(SerializedProperty dataProperty, UnityEngine.Object obj)
        {
            string key = "";
            UnityEngine.Object targetComponent = obj;

            // 如果是 GameObject，尝试获取 UI 组件
            if (obj is GameObject go)
            {
                // 按优先级检查组件类型
                if (go.TryGetComponent<Button>(out var button))
                {
                    key = $"Btn{GetFieldName(go.name)}";
                    targetComponent = button;
                }
                else if (go.TryGetComponent<TextMeshProUGUI>(out var tmp))
                {
                    key = $"TMP{GetFieldName(go.name)}";
                    targetComponent = tmp;
                }
                else if (go.TryGetComponent<Image>(out var img))
                {
                    key = $"Img{GetFieldName(go.name)}";
                    targetComponent = img;
                }
                else if (go.TryGetComponent<ScrollRect>(out var scrollRect))
                {
                    key = $"ScrollRect{GetFieldName(go.name)}";
                    targetComponent = scrollRect;
                }
                else if (go.TryGetComponent<Text>(out var text))
                {
                    key = $"Txt{GetFieldName(go.name)}";
                    targetComponent = text;
                }
                else
                {
                    // 没有识别的组件类型，直接使用 GameObject 名称
                    key = GetFieldName(go.name);
                    targetComponent = go;
                }
            }
            // 如果直接拖入的是组件
            else if (obj is Component component)
            {
                var goName = component.gameObject.name;
                if (component is Button)
                {
                    key = $"Btn{GetFieldName(goName)}";
                }
                else if (component is TextMeshProUGUI)
                {
                    key = $"TMP{GetFieldName(goName)}";
                }
                else if (component is Image)
                {
                    key = $"Img{GetFieldName(goName)}";
                }
                else if (component is ScrollRect)
                {
                    key = $"ScrollRect{GetFieldName(goName)}";
                }
                else if (component is Text)
                {
                    key = $"Txt{GetFieldName(goName)}";
                }
                else
                {
                    key = GetFieldName(goName);
                }
                targetComponent = component;
            }
            else
            {
                // 其他类型的对象
                key = obj.name;
            }

            // 处理重名，确保名称唯一
            key = GetUniqueName(key);

            AddReference(dataProperty, key, targetComponent);
        }

        /// <summary>
        /// 添加新的引用
        /// </summary>
        private void AddReference(SerializedProperty dataProperty, string key, UnityEngine.Object obj)
        {
            int index = dataProperty.arraySize;
            dataProperty.InsertArrayElementAtIndex(index);
            var element = dataProperty.GetArrayElementAtIndex(index);
            element.FindPropertyRelative("key").stringValue = key;
            element.FindPropertyRelative("gameObject").objectReferenceValue = obj;
        }

        /// <summary>
        /// 获取规范化的字段名（不处理重名）
        /// </summary>
        private static string GetFieldName(string objectName)
        {
            // 1. 修剪前后空格
            var result = objectName.Trim();

            // 2. 替换非单词字符为下划线
            result = Regex.Replace(result, @"[^\w]", "_");

            // 3. 将连续的下划线合并为一个
            result = Regex.Replace(result, @"_+", "_");

            // 4. 去掉开头和结尾的下划线
            result = result.Trim('_');

            // 5. 转换成驼峰命名（去掉所有下划线，下划线后的字母大写）
            result = ConvertToPascalCase(result);

            return result;
        }

        /// <summary>
        /// 将带下划线的字符串转换为驼峰命名（PascalCase）
        /// </summary>
        private static string ConvertToPascalCase(string input)
        {
            if (string.IsNullOrEmpty(input))
                return input;

            // 按下划线分割
            var parts = input.Split('_');
            var result = new System.Text.StringBuilder();

            foreach (var part in parts)
            {
                if (string.IsNullOrEmpty(part))
                    continue;

                // 首字母大写，其余保持原样
                result.Append(char.ToUpper(part[0]));
                if (part.Length > 1)
                    result.Append(part.Substring(1));
            }

            return result.ToString();
        }

        /// <summary>
        /// 获取唯一的名称（处理重名情况）
        /// </summary>
        /// <param name="baseName">基础名称</param>
        /// <param name="excludeKey">要排除检查的key（通常是当前正在编辑的项）</param>
        private string GetUniqueName(string baseName, string excludeKey = null)
        {
            // 检查当前 ReferenceCollector 中是否已存在（排除指定的key）
            if (!referenceCollector.data.Exists(x => x.key == baseName && x.key != excludeKey))
            {
                return baseName;
            }

            // 如果存在，添加数字后缀（从2开始）
            int suffix = 2;
            string uniqueName;
            do
            {
                uniqueName = $"{baseName}{suffix}";
                suffix++;
            } while (referenceCollector.data.Exists(x => x.key == uniqueName && x.key != excludeKey));

            return uniqueName;
        }

        /// <summary>
        /// 根据组件类型生成对应的key名称
        /// </summary>
        private string GenerateKeyForComponent(UnityEngine.Object component, string gameObjectName)
        {
            string baseName = GetFieldName(gameObjectName);

            if (component is GameObject)
            {
                return baseName;
            }
            else if (component is Button)
            {
                return AddPrefixIfNeeded("Btn", baseName);
            }
            else if (component is TextMeshProUGUI)
            {
                return AddPrefixIfNeeded("TMP", baseName);
            }
            else if (component is Image)
            {
                return AddPrefixIfNeeded("Img", baseName);
            }
            else if (component is ScrollRect)
            {
                return AddPrefixIfNeeded("ScrollRect", baseName);
            }
            else if (component is Text)
            {
                return AddPrefixIfNeeded("Txt", baseName);
            }
            else if (component is RectTransform)
            {
                return AddPrefixIfNeeded("Rect", baseName);
            }
            else if (component is Transform)
            {
                return AddPrefixIfNeeded("Trans", baseName);
            }
            else if (component is Component)
            {
                // 其他组件类型，使用组件类型名作为前缀
                string typeName = component.GetType().Name;
                return AddPrefixIfNeeded(typeName, baseName);
            }
            else
            {
                return baseName;
            }
        }

        /// <summary>
        /// 如果baseName不以prefix开头，则添加前缀；否则直接返回baseName
        /// </summary>
        private static string AddPrefixIfNeeded(string prefix, string baseName)
        {
            // 如果baseName已经以prefix开头（不区分大小写），则不添加前缀
            if (baseName.StartsWith(prefix, System.StringComparison.OrdinalIgnoreCase))
            {
                // 确保首字母大写匹配
                return prefix + baseName.Substring(prefix.Length);
            }
            return $"{prefix}{baseName}";
        }

        /// <summary>
        /// 绘制动态组件类型下拉菜单
        /// </summary>
        private void DrawComponentTypeDropdown(SerializedProperty objProperty, SerializedProperty keyProperty, UnityEngine.Object currentObj, int index)
        {
            GameObject go = null;
            Component currentComponent = null;

            // 获取 GameObject
            if (currentObj is GameObject gameObject)
            {
                go = gameObject;
                currentComponent = go.transform;
            }
            else if (currentObj is Component component)
            {
                go = component.gameObject;
                currentComponent = component;
            }

            if (go == null)
            {
                EditorGUILayout.LabelField("(无效)", GUILayout.Width(120));
                return;
            }

            // 获取所有组件
            var components = go.GetComponents<Component>();
            var componentNames = new List<string>();
            var componentList = new List<UnityEngine.Object>();

            // 添加 GameObject 选项
            componentNames.Add("GameObject");
            componentList.Add(go);

            // 添加所有组件
            foreach (var comp in components)
            {
                if (comp == null) continue;
                componentNames.Add(comp.GetType().Name);
                componentList.Add(comp);
            }

            // 找到当前选择的索引
            int selectedIndex = 0;

            // 如果当前对象是GameObject
            if (currentObj is GameObject)
            {
                selectedIndex = 0;
            }
            // 如果当前对象是组件
            else
            {
                for (int j = 1; j < componentList.Count; j++)
                {
                    if (componentList[j] == currentComponent)
                    {
                        selectedIndex = j;
                        break;
                    }
                }
            }

            // 显示下拉菜单
            int newIndex = EditorGUILayout.Popup(selectedIndex, componentNames.ToArray(), GUILayout.Width(120));

            // 如果选择改变，更新引用和key
            if (newIndex != selectedIndex && newIndex >= 0 && newIndex < componentList.Count)
            {
                UnityEngine.Object newComponent;
                if (newIndex == 0) // GameObject
                {
                    newComponent = go;
                    objProperty.objectReferenceValue = go;
                }
                else
                {
                    newComponent = componentList[newIndex];
                    objProperty.objectReferenceValue = componentList[newIndex];
                }

                // 更新key名称（排除当前项自己，避免重名检测冲突）
                string oldKey = keyProperty.stringValue;
                string newKey = GenerateKeyForComponent(newComponent, go.name);
                newKey = GetUniqueName(newKey, oldKey);
                keyProperty.stringValue = newKey;

                EditorUtility.SetDirty(referenceCollector);
            }
        }

        /// <summary>
        /// 修正所有key的命名
        /// </summary>
        private void FixAllKeyNames()
        {
            SerializedObject serializedObject = new SerializedObject(referenceCollector);
            SerializedProperty dataProperty = serializedObject.FindProperty("data");

            // 用于存储所有新的key名称，确保唯一性
            var usedKeys = new HashSet<string>();
            var keyUpdates = new List<(int index, string newKey)>();

            // 第一遍：为每个引用生成新的key名称
            for (int i = 0; i < referenceCollector.data.Count; i++)
            {
                var itemData = referenceCollector.data[i];
                var obj = itemData.gameObject;

                if (obj == null)
                    continue;

                GameObject go = null;
                Component component = null;

                // 获取GameObject和组件
                if (obj is GameObject gameObject)
                {
                    go = gameObject;
                }
                else if (obj is Component comp)
                {
                    go = comp.gameObject;
                    component = comp;
                }

                if (go == null)
                    continue;

                // 根据组件类型生成key
                string newKey = GenerateKeyForComponent(obj, go.name);

                // 处理重名
                string uniqueKey = newKey;
                int suffix = 2;
                while (usedKeys.Contains(uniqueKey))
                {
                    uniqueKey = $"{newKey}{suffix}";
                    suffix++;
                }

                usedKeys.Add(uniqueKey);
                keyUpdates.Add((i, uniqueKey));
            }

            // 第二遍：更新所有key
            for (int i = 0; i < keyUpdates.Count; i++)
            {
                var (index, newKey) = keyUpdates[i];
                var element = dataProperty.GetArrayElementAtIndex(index);
                var keyProperty = element.FindPropertyRelative("key");

                if (keyProperty.stringValue != newKey)
                {
                    keyProperty.stringValue = newKey;
                }
            }

            EditorUtility.SetDirty(referenceCollector);
            serializedObject.ApplyModifiedProperties();
            serializedObject.UpdateIfRequiredOrScript();

            Debug.Log($"Fixed {keyUpdates.Count} key names.");
        }

        /// <summary>
        /// 生成 UI 脚本
        /// </summary>
        private void GenerateUIScript()
        {
            var go = referenceCollector.gameObject;

            // 调用 AutoGeneratePanelScript 的生成方法
            var menuCommand = new MenuCommand(go);
            AutoGeneratePanelScript.GeneratePanelScripts(menuCommand);
        }
    }
}

#endif