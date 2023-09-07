//使用utf-8
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CommonBase
{
    public class RayUtility
    {
        public static bool RayIntersectionOnPlane(Ray ray, Plane plane, out Vector3 intersection)
        {
            if (plane.Raycast(ray, out var distance))
            {
                intersection = ray.GetPoint(distance);
                return true;
            }
            intersection = Vector3.zero;
            return false;
        }
    }
}

