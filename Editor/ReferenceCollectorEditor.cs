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

        // 用于记录已使用的变量名及其出现次数
        private static readonly Dictionary<string, int> UsedFieldNames = new();

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
            EditorGUILayout.PropertyField(generationTypeProperty, new GUIContent("生成类型", "Panel: 完整的UI面板（继承BaseUI）\nWidget: UI小部件（实现IUIWidget接口）"));
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
                    DrawComponentTypeDropdown(objProperty, currentObj, i);
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
        /// 获取规范化的字段名
        /// </summary>
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

        /// <summary>
        /// 绘制动态组件类型下拉菜单
        /// </summary>
        private void DrawComponentTypeDropdown(SerializedProperty objProperty, UnityEngine.Object currentObj, int index)
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
            var componentList = new List<Component>();

            // 添加 GameObject 选项
            componentNames.Add("GameObject");
            componentList.Add(go.transform);

            // 添加所有组件
            foreach (var comp in components)
            {
                if (comp == null) continue;
                componentNames.Add(comp.GetType().Name);
                componentList.Add(comp);
            }

            // 找到当前选择的索引
            int selectedIndex = 0;
            for (int j = 0; j < componentList.Count; j++)
            {
                if (componentList[j] == currentComponent)
                {
                    selectedIndex = j;
                    break;
                }
            }

            // 显示下拉菜单
            int newIndex = EditorGUILayout.Popup(selectedIndex, componentNames.ToArray(), GUILayout.Width(120));

            // 如果选择改变，更新引用
            if (newIndex != selectedIndex && newIndex >= 0 && newIndex < componentList.Count)
            {
                if (newIndex == 0) // GameObject
                {
                    objProperty.objectReferenceValue = go;
                }
                else
                {
                    objProperty.objectReferenceValue = componentList[newIndex];
                }
                EditorUtility.SetDirty(referenceCollector);
            }
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