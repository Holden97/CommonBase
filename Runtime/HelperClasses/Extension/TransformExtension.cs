using System.Collections.Generic;
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

        /// <summary>
        /// Finds children by name, breadth first
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="transformName"></param>
        /// <returns></returns>
        public static Transform FindDeepChildBreadthFirst(this Transform parent, string transformName)
        {
            Queue<Transform> queue = new Queue<Transform>();
            queue.Enqueue(parent);
            while (queue.Count > 0)
            {
                Transform child = queue.Dequeue();
                if (child.name == transformName)
                {
                    return child;
                }
                foreach (Transform t in child)
                {
                    queue.Enqueue(t);
                }
            }
            return null;
        }

        /// <summary>
        /// Finds children by name, depth first
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="transformName"></param>
        /// <returns></returns>
        public static Transform FindDeepChildDepthFirst(this Transform parent, string transformName)
        {
            foreach (Transform child in parent)
            {
                if (child.name == transformName)
                {
                    return child;
                }

                Transform result = child.FindDeepChildDepthFirst(transformName);
                if (result != null)
                {
                    return result;
                }
            }
            return null;
        }

        public static void ChangeLayer(this Transform trans, string targetLayer)
        {
            if (LayerMask.NameToLayer(targetLayer) == -1)
            {
                Debug.Log("Layer中不存在,请手动添加LayerName");

                return;
            }
            //遍历更改所有子物体layer
            trans.gameObject.layer = LayerMask.NameToLayer(targetLayer);
            foreach (Transform child in trans)
            {
                ChangeLayer(child, targetLayer);
                Debug.Log(child.name + "子对象Layer更改成功！");
            }
        }
    }
}
