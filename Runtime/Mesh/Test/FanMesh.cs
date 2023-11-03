using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CommonBase
{
    /// <summary>
    /// 扇形生成器
    /// </summary>
    public class FanMesh : AbstractMesh
    {
        [Range(0, 360)] public float centerLineDegree;
        /// <summary>
        /// 展开弧度
        /// </summary>
        [Range(0, 360)] public float arcDegree;
        [Range(0.1f, 10)] public float radius = 1;

        private float beginAngle;
        private float endAngle;

        public FanMesh(float centerLineDegree, float arcDegree, float radius, Vector3 position, Quaternion quaternion, Vector3 scale, Material material) : base(position, quaternion, scale, material)
        {
            this.centerLineDegree = centerLineDegree;
            this.arcDegree = arcDegree;
            this.radius = radius;
        }

        protected override void SetVertices()
        {
            vertices.Add(Vector3.zero);

            vertices.Add(GetVertice(beginAngle));

            float i = beginAngle;
            for (i = beginAngle + 1; i < endAngle; i++)
            {
                vertices.Add(GetVertice(i));
            }

            vertices.Add(GetVertice(endAngle));
        }

        protected override void SetMeshNums()
        {
            //确保展开弧度为非负数
            var realArcDegree = Mathf.Abs(arcDegree);
            if (arcDegree == 0) { realArcDegree = 0.001f; }
            beginAngle = centerLineDegree - realArcDegree / 2;
            endAngle = centerLineDegree + realArcDegree / 2;

            var pointsOnCurve = Mathf.Max(Mathf.CeilToInt(realArcDegree + 1), 2);
            numVertices = pointsOnCurve + 1;
            numTriangles = (pointsOnCurve - 1) * 3;
        }

        private Vector3 GetVertice(float degree)
        {
            var rad = Mathf.Deg2Rad * degree;
            return (new Vector3(Mathf.Cos(rad) * radius, Mathf.Sin(rad) * radius, 0));
        }

        protected override void SetTriangles()
        {
            for (int i = 1 + 1; i < vertices.Count; i++)
            {
                triangles.Add(0);
                triangles.Add(i);
                triangles.Add(i - 1);
            }
        }

        public enum FanFoldCenter
        {
            LEFT,
            CENTER,
            RIGHT,
        }
    }
}
