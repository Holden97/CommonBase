using System.Collections;
using System.Collections.Generic;
using UnityEditor;
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

        public static void DrawCircle(Vector3 center, float radius, Color color)
        {
#if UNITY_EDITOR
            int segments = 36; // 圆形的线段数
            Handles.color = color;  // 设置圆形的颜色
            for (int i = 0; i < segments; i++)
            {
                float angle1 = (i / (float)segments) * 2 * Mathf.PI;
                float angle2 = ((i + 1) / (float)segments) * 2 * Mathf.PI;
                Vector3 point1 = center + new Vector3(Mathf.Cos(angle1) * radius, 0, Mathf.Sin(angle1) * radius);
                Vector3 point2 = center + new Vector3(Mathf.Cos(angle2) * radius, 0, Mathf.Sin(angle2) * radius);

                Handles.DrawLine(point1, point2);
            }
#endif
        }
    }
}

