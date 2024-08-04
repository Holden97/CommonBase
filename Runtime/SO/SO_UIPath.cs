using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CommonBase
{
    /// <summary>
    /// 扫描项目中所有的带BaseUI的预制体，
    /// </summary>
    [CreateAssetMenu(fileName = "SO_UIPath", menuName = "Scriptable Object/UIPath")]
    public class SO_UIPath : ScriptableObject
    {
        public List<UIInfo> uIInfos;
    }

    [Serializable]
    public class UIInfo
    {
        public string name;
        //public GameObject uiPrefab;
        public UIType uiType;
        public PanelFadeType fadeType;
        public int orderInLayer;
        public bool ecsRemovable;

        public UIInfo(string name, GameObject uiPrefab)
        {
            this.name = name;
            //this.uiPrefab = uiPrefab;
            this.uiType = uiPrefab.GetComponent<BaseUI>().uiLayer;
            this.orderInLayer = uiPrefab.GetComponent<BaseUI>().orderInLayer;
            this.fadeType = uiPrefab.GetComponent<BaseUI>().fadeType;
            this.ecsRemovable = uiPrefab.GetComponent<BaseUI>().escRemovable;
        }
    }
}
