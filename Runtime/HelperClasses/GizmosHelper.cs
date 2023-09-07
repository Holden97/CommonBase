using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CommonBase
{
    public class GizmosHelper
    {
        /// <summary>
        /// 基于Unity世界坐标绘制矩形
        /// </summary>
        public static void DrawRect(Vector3 center, float width, float height)
        {
            var rectCenter = center;
            var p1 = rectCenter + new Vector3(-width, -height) * 0.5f;
            var p2 = rectCenter + new Vector3(-width, height) * 0.5f;
            var p3 = rectCenter + new Vector3(width, height) * 0.5f;
            var p4 = rectCenter + new Vector3(width, -height) * 0.5f;
            Gizmos.DrawLine(p1, p2);
            Gizmos.DrawLine(p2, p3);
            Gizmos.DrawLine(p3, p4);
            Gizmos.DrawLine(p4, p1);
        }
    }
}

