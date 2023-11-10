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
    }
}
