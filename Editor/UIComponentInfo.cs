#if UNITY_EDITOR
using UnityEngine;

public class UIComponentInfo
{
    public UIComponentInfo(Component component, string path, string name)
    {
        this.component = component;
        this.path = path;
        this.name = name;
        this.typeString = "";
    }

    public UIComponentInfo(Object obj, string path, string name, string typeString)
    {
        // 如果是GameObject，取其Transform作为component
        if (obj is GameObject go)
        {
            this.component = go.transform;
        }
        else
        {
            this.component = obj as Component;
        }
        this.path = path;
        this.name = name;
        this.typeString = typeString;
    }

    public Component component { get; set; }
    public string path { get; set; }
    public string name { get; set; }
    public string typeString { get; set; }
}
#endif