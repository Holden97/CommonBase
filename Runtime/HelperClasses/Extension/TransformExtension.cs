using UnityEngine;

namespace CommonBase
{
    public static class TransformExtension
    {
        public static void DestroyChildren(this Transform transform)
        {
            foreach (var item in transform)
            {
                Object.Destroy(((Transform)item).gameObject);
            }
        }

        public static void DestroyChildren(this Transform transform, string childName)
        {
            foreach (var item in transform)
            {
                if (((Transform)item).name.Contains(childName))
                {
                    Object.Destroy(((Transform)item).gameObject);
                }
            }
        }

        public static T GetComponentInSibling<T>(this Component c) where T : Component
        {
            for (int i = 0; i < c.transform.parent.childCount; i++)
            {
                var curComponent = c.transform.parent.GetChild(i).GetComponent<T>();
                if (curComponent != null)
                {
                    return curComponent;
                }
            }
            return null;
        }
    }
}
