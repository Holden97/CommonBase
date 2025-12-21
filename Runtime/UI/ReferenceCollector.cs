#if UNITY_EDITOR
using UnityEditor;
#endif
using System;
using System.Collections.Generic;
using UnityEngine;

namespace CommonBase
{
    /// <summary>
    /// UI 生成类型
    /// </summary>
    public enum UIGenerationType
    {
        /// <summary>
        /// 完整的 UI Panel，继承 BaseUI
        /// </summary>
        Panel,
        /// <summary>
        /// UI 小部件，继承 MonoBehaviour 并实现 IUIWidget
        /// </summary>
        Widget
    }

    /// <summary>
    /// 用于存储引用的数据项
    /// </summary>
    [Serializable]
    public class ReferenceCollectorData
    {
        public string key = "";
        public UnityEngine.Object gameObject = null;
    }

    /// <summary>
    /// 引用数据比较器，用于排序
    /// </summary>
    public class ReferenceCollectorDataComparer : IComparer<ReferenceCollectorData>
    {
        public int Compare(ReferenceCollectorData x, ReferenceCollectorData y)
        {
            return string.Compare(x.key, y.key, StringComparison.Ordinal);
        }
    }

    /// <summary>
    /// 引用收集器组件，用于收集和管理UI组件引用
    /// 通过此组件可以手动指定需要生成脚本引用的UI组件，而不是自动收集所有子物体
    /// </summary>
    public class ReferenceCollector : MonoBehaviour, ISerializationCallbackReceiver
    {
        // 存储所有引用数据
        public List<ReferenceCollectorData> data = new List<ReferenceCollectorData>();

        // UI 生成类型
        public UIGenerationType generationType = UIGenerationType.Panel;

        // 运行时字典，用于快速查找
        private readonly Dictionary<string, UnityEngine.Object> _dict = new Dictionary<string, UnityEngine.Object>();

#if UNITY_EDITOR
        /// <summary>
        /// 添加新的引用
        /// </summary>
        public void Add(string key, UnityEngine.Object obj)
        {
            SerializedObject serializedObject = new SerializedObject(this);
            SerializedProperty dataProperty = serializedObject.FindProperty("data");
            int i;

            // 检查是否已存在相同的key
            for (i = 0; i < data.Count; i++)
            {
                if (data[i].key == key)
                {
                    break;
                }
            }

            // 如果key已存在，更新对应的对象
            if (i != data.Count)
            {
                SerializedProperty element = dataProperty.GetArrayElementAtIndex(i);
                element.FindPropertyRelative("gameObject").objectReferenceValue = obj;
            }
            else
            {
                // 否则添加新元素
                dataProperty.InsertArrayElementAtIndex(i);
                SerializedProperty element = dataProperty.GetArrayElementAtIndex(i);
                element.FindPropertyRelative("key").stringValue = key;
                element.FindPropertyRelative("gameObject").objectReferenceValue = obj;
            }

            EditorUtility.SetDirty(this);
            serializedObject.ApplyModifiedProperties();
            serializedObject.UpdateIfRequiredOrScript();
        }

        /// <summary>
        /// 移除指定key的引用
        /// </summary>
        public void Remove(string key)
        {
            SerializedObject serializedObject = new SerializedObject(this);
            SerializedProperty dataProperty = serializedObject.FindProperty("data");
            int i;

            for (i = 0; i < data.Count; i++)
            {
                if (data[i].key == key)
                {
                    break;
                }
            }

            if (i != data.Count)
            {
                dataProperty.DeleteArrayElementAtIndex(i);
            }

            EditorUtility.SetDirty(this);
            serializedObject.ApplyModifiedProperties();
            serializedObject.UpdateIfRequiredOrScript();
        }

        /// <summary>
        /// 清空所有引用
        /// </summary>
        public void Clear()
        {
            SerializedObject serializedObject = new SerializedObject(this);
            var dataProperty = serializedObject.FindProperty("data");
            dataProperty.ClearArray();
            EditorUtility.SetDirty(this);
            serializedObject.ApplyModifiedProperties();
            serializedObject.UpdateIfRequiredOrScript();
        }

        /// <summary>
        /// 对引用列表进行排序
        /// </summary>
        public void Sort()
        {
            SerializedObject serializedObject = new SerializedObject(this);
            data.Sort(new ReferenceCollectorDataComparer());
            EditorUtility.SetDirty(this);
            serializedObject.ApplyModifiedProperties();
            serializedObject.UpdateIfRequiredOrScript();
        }
#endif

        /// <summary>
        /// 获取指定key的对象（泛型版本）
        /// </summary>
        public T Get<T>(string key) where T : UnityEngine.Object
        {
            UnityEngine.Object dictGo;
            if (!_dict.TryGetValue(key, out dictGo))
            {
                return null;
            }
            return (T)dictGo;
        }

        /// <summary>
        /// 获取指定key的对象
        /// </summary>
        public UnityEngine.Object GetObject(string key)
        {
            UnityEngine.Object dictGo;
            if (!_dict.TryGetValue(key, out dictGo))
            {
                return null;
            }
            return dictGo;
        }

        public void OnBeforeSerialize()
        {
        }

        /// <summary>
        /// 反序列化后，将数据转换为字典以便快速查找
        /// </summary>
        public void OnAfterDeserialize()
        {
            _dict.Clear();
            foreach (ReferenceCollectorData referenceCollectorData in data)
            {
                if (!_dict.ContainsKey(referenceCollectorData.key))
                {
                    _dict.Add(referenceCollectorData.key, referenceCollectorData.gameObject);
                }
            }
        }
    }
}