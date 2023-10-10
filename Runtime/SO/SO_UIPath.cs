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
        public string resPath;
        public GameObject uiPrefab;
    }
}
