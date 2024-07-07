using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CommonBase
{
    public static class Graph2DExtension
    {
        public static bool IsPointInCircle(this Vector2 point, Vector2 circleCenter, float radius)
        {
            return Vector2.SqrMagnitude(circleCenter - point) <= radius * radius;
        }

        public static bool IsPointInCircle(this Vector2Int point, Vector2 circleCenter, float radius)
        {
            return Vector2.SqrMagnitude(circleCenter - point) <= radius * radius;
        }
    }
}
