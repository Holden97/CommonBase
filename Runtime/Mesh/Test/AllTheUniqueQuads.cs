using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CommonBase
{
    public class AllTheUniqueQuads : AbstractMonoMeshGenerator
    {
        [SerializeField]
        private Vector3[] vs = new Vector3[6];
        [SerializeField]
        private Vector2[] flexibleUVs = new Vector2[6];
        protected override void SetMeshNums()
        {
            numVertices = 6;
            numTriangles = 6;
        }

        protected override void SetNormals()
        {
        }

        protected override void SetTangents()
        {
        }

        protected override void SetTriangles()
        {
            triangles.Add(0);
            triangles.Add(1);
            triangles.Add(2);

            triangles.Add(3);
            triangles.Add(4);
            triangles.Add(5);
        }

        protected override void SetUVs()
        {
            uvs.AddRange(flexibleUVs);
        }

        protected override void SetVertexColours()
        {
        }

        protected override void SetVertices()
        {
            vertices.Add(vs[0]);
            vertices.Add(vs[1]);
            vertices.Add(vs[3]);
            vertices.Add(vs[0]);
            vertices.Add(vs[3]);
            vertices.Add(vs[2]);
        }
    }
}
