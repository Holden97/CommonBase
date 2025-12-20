#if UNITY_EDITOR
using UnityEngine;

public class UIComponentInfo
{
    public UIComponentInfo(MonoBehaviour component, string path, string name)
    {
        this.component = component;
        this.path = path;
        this.name = name;
    }

    public MonoBehaviour component { get; set; }
    public string path { get; set; }
    public string name { get; set; }
}
#endif