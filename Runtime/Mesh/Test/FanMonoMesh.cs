using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CommonBase
{
    /// <summary>
    /// 扇形生成器
    /// </summary>
    public class FanMonoMesh : AbstractMonoMeshGenerator
    {
        [Range(0, 360)] public float centerLineDegree;
        /// <summary>
        /// 展开弧度
        /// </summary>
        [Range(0, 360)] public float arcDegree;
        [Range(0.1f, 10)] public float radius = 1;
        private float beginAngle;
        private float endAngle;
        public Vector3 centerPoint;

        public bool isFoldingIn = false;
        public float anglePerTick = 0;

        public FanFoldCenter fanFoldType;

        protected override void SetVertices()
        {
            vertices.Add(centerPoint);

            vertices.Add(GetVertice(beginAngle) + centerPoint);

            float i = beginAngle;
            for (i = beginAngle + 1; i < endAngle; i++)
            {
                vertices.Add(GetVertice(i) + centerPoint);
            }

            vertices.Add(GetVertice(endAngle) + centerPoint);
        }

        protected override void Update()
        {
            if (isFoldingIn)
            {
                arcDegree -= anglePerTick;
            }
            base.Update();
        }

        public void FoldIn(float originalAgree, float duration)
        {
            StartCoroutine(FoldInInside(originalAgree, duration));
        }

        public IEnumerator FoldInInside(float originalAgree, float duration)
        {
            this.arcDegree = originalAgree;
            isFoldingIn = true;
            anglePerTick = arcDegree / duration;
            yield return new WaitForSeconds(duration);
            isFoldingIn = false;
        }

        public void FoldIn(float duration)
        {
            StartCoroutine(FoldInInside(duration));
        }

        public IEnumerator FoldInInside(float duration)
        {
            isFoldingIn = true;
            anglePerTick = arcDegree / duration;
            yield return new WaitForSeconds(duration);
            isFoldingIn = false;
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

        public void DrawOutLine()
        {
            MeshUtil.DrawFanMeshLine(this.gameObject, mesh);
        }

        public void DrawPredictionLine(float originalAgree)
        {
            this.arcDegree = originalAgree;
        }

        public enum FanFoldCenter
        {
            LEFT,
            CENTER,
            RIGHT,
        }
    }
}
