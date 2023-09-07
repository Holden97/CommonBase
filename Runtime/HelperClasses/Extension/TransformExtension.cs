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
    }
}
