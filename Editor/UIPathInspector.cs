using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace CommonBase.Editor
{
    [CustomEditor(typeof(SO_UIPath))]
    public class UIPathInspector : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            if (GUILayout.Button("干预"))
            {

                SO_UIPath uiInfo = target as SO_UIPath;

                foreach (UIInfo ui in uiInfo.uIInfos)
                {
                    if (ui.uiPrefab != null)
                    {
                        var uiComponent = ui.uiPrefab.GetComponent<BaseUI>();
                        uiComponent.uiLayer = ui.uiType;
                        uiComponent.fadeType = ui.fadeType;
                        uiComponent.ecsRemovable = ui.ecsRemovable;
                        EditorUtility.SetDirty(ui.uiPrefab);
                    }
                }

                // 保存并刷新资源数据库
                EditorUtility.SetDirty(uiInfo);

                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
                Debug.Log("UI信息更新结束!");
            }
        }
    }
}

