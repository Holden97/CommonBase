using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace CommonBase
{
    public static class GizmosHelper
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

        public static void DrawCircleOnGizmos(this Transform m_Transform, float m_Radius, Color m_Color, float m_Theta = 0.1f)
        {
            if (m_Transform == null) return;
            if (m_Theta < 0.0001f) m_Theta = 0.0001f;

            // 设置矩阵
            Matrix4x4 defaultMatrix = Gizmos.matrix;
            Gizmos.matrix = m_Transform.localToWorldMatrix;

            // 设置颜色
            Color defaultColor = Gizmos.color;
            Gizmos.color = m_Color;

            // 绘制圆环
            Vector3 beginPoint = Vector3.zero;
            Vector3 firstPoint = Vector3.zero;
            for (float theta = 0; theta < 2 * Mathf.PI; theta += m_Theta)
            {
                float x = m_Radius * Mathf.Cos(theta);
                float z = m_Radius * Mathf.Sin(theta);
                Vector3 endPoint = new Vector3(x, 0, z);
                if (theta == 0)
                {
                    firstPoint = endPoint;
                }
                else
                {
                    Gizmos.DrawLine(beginPoint, endPoint);
                }
                beginPoint = endPoint;
            }

            // 绘制最后一条线段
            Gizmos.DrawLine(firstPoint, beginPoint);
            // 恢复默认颜色
            Gizmos.color = defaultColor;
            // 恢复默认矩阵
            Gizmos.matrix = defaultMatrix;
        }

    }
}

