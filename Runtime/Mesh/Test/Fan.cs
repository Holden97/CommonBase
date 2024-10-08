﻿//使用utf-8
using UnityEngine;

namespace CommonBase
{
    public class Fan
    {
        public Vector3 center;
        /// <summary>
        /// 扇形半径长度
        /// </summary>
        public float radius;
        /// <summary>
        /// 扇形弧度
        /// </summary>
        public float radian;
        /// <summary>
        /// 扇形中线角度（角度制）
        /// </summary>
        public float centerlineDegree;

        public Fan(Vector3 center, float radian, float radius, float centerlineDegree)
        {
            this.center = center;
            this.radius = radius;
            this.radian = radian;
            this.centerlineDegree = centerlineDegree;
        }

        public Fan(Vector3 center, float radian, float radius, Vector3 centerLineDir)
        {
            this.center = center;
            this.radius = radius;
            this.radian = radian;
            var rawDir = centerLineDir;
            this.centerlineDegree = Mathf.Atan2(rawDir.y, rawDir.x);
        }

        /// <summary>
        /// 点是否在扇形内
        /// 1.点据终点距离小于半径
        /// 2.点与中点形成向量与扇形中线点乘，所得角度小于1/2的半径
        /// </summary>
        /// <param name="target"></param>
        /// <returns></returns>
        public bool PointInRange(Vector3 target)
        {
            if (target == center) return true;
            var shorter = Vector3.Distance(target, center) < radius;
            var targetAngleNormalized = new Vector3(Mathf.Cos(centerlineDegree), Mathf.Sin(centerlineDegree)).normalized;
            var dot = Vector2.Dot((target - center).normalized, targetAngleNormalized);
            var lessThanHalfRadius = Mathf.Acos(dot) < radian / 2;
            return shorter && lessThanHalfRadius;
        }
    }
}