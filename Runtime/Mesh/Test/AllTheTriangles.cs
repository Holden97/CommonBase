using System;
using System.Collections.Generic;
using UnityEngine;

namespace CommonBase
{
    public class AllTheTriangles : AbstractMonoMeshGenerator
    {
        [SerializeField]
        private Vector3[] vs = new Vector3[3];

        protected override void SetTriangles()
        {
            vertices.AddRange(vs);
        }

        protected override void SetVertices()
        {
            triangles.Add(0);
            triangles.Add(1);
            triangles.Add(2);
        }

        protected override void SetMeshNums()
        {
            numVertices = 3;
            numTriangles = 3;
        }

        protected override void SetNormals()
        {
        }

        protected override void SetTangents()
        {
        }

        protected override void SetUVs()
        {
        }

        protected override void SetVertexColours()
        {
        }
    }
}
