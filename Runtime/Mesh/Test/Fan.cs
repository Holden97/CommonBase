//使用utf-8
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace CommonBase
{
    public class Fan
    {
        public Vector3 center;
        public float radius;
        public float arcDegree;
        public float centerlineDegree;

        public Fan(Vector3 center, float radius, float arcDegree, float centerlineDegree)
        {
            this.center = center;
            this.radius = radius;
            this.arcDegree = arcDegree;
            this.centerlineDegree = centerlineDegree;
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
            var dot = Vector3.Dot((target - center).normalized, new Vector3(Mathf.Cos(Mathf.Deg2Rad * arcDegree), Mathf.Sin(Mathf.Deg2Rad * arcDegree)));
            var lessThanHalfRadius = Mathf.Acos(dot) < radius / 2;
            return shorter && lessThanHalfRadius;
        }
    }
}