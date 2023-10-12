//使用utf-8
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace CommonBase.Editor
{
    public class UIPathEditor : UnityEditor.Editor
    {
        [MenuItem("Tools/更新UI信息")]
        public static void CreateScriptableObject()
        {
            SO_UIPath uiPath = Resources.Load<SO_UIPath>("SO/UIPath.asset");
            if (uiPath == null)
            {
                uiPath = CreateNew();
            }


            uiPath.uIInfos = new List<UIInfo>();

            Debug.Log("BaseUI查找开始");
            Type scriptType = typeof(BaseUI);

            if (scriptType == null)
            {
                Debug.LogError("Script type not found: BaseUI");
                return;
            }

            // 使用过滤条件来搜索带有特定脚本的预设
            string[] guids = AssetDatabase.FindAssets("t:Prefab");

            foreach (string guid in guids)
            {
                string assetPath = AssetDatabase.GUIDToAssetPath(guid);
                GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);

                if (prefab != null && prefab.TryGetComponent<BaseUI>(out var comp))
                {
                    uiPath.uIInfos.Add(new UIInfo(comp.name, assetPath, prefab));
                }
            }


            // 保存并刷新资源数据库
            EditorUtility.SetDirty(uiPath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log("UI信息更新结束!");
        }

        private static SO_UIPath CreateNew()
        {
            // 创建一个新的 ScriptableObject 实例
            SO_UIPath newScriptableObject = CreateInstance<SO_UIPath>();

            // 创建一个保存路径
            string path = "Assets/Resources/SO/";

            // 如果目录不存在，创建它
            if (!System.IO.Directory.Exists(path))
            {
                System.IO.Directory.CreateDirectory(path);
            }

            // 设置保存路径并创建 Asset 文件
            string assetPathAndName = AssetDatabase.GenerateUniqueAssetPath(path + "UIPath.asset");
            AssetDatabase.CreateAsset(newScriptableObject, assetPathAndName);
            return AssetDatabase.LoadAssetAtPath<SO_UIPath>(assetPathAndName);
        }
    }
}