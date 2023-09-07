using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CommonBase
{
    public static class GameObjectExtension
    {
        public static T GetOrAddComponent<T>(this GameObject uo) where T : Component
        {
            return uo.GetComponent<T>() ?? uo.AddComponent<T>();
        }

        public static T GetOrAddCompnentInChildren<T>(this GameObject panel, string name) where T : Component
        {
            GameObject child = FindChildGameObject(panel, name);
            if (child)
            {
                if (child.GetComponent<T>() == null)
                    child.GetComponent<T>();

                return child.GetComponent<T>();
            }
            Debug.LogError($"{panel.name}找不到名为{name}的子组件");
            return null;
        }

        private static GameObject FindChildGameObject(this GameObject panel, string name)
        {
            Transform[] trans = panel.GetComponentsInChildren<Transform>();
            foreach (var item in trans)
            {
                if (item.name == name)
                {
                    return item.gameObject;
                }
            }

            Debug.LogWarning($"{panel.name}找不到名为{name}的子对象");
            return null;
        }
    }

}
