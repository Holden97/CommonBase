using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CommonBase
{
    public class AllTheSharedVertTetrahedrons : AbstractMonoMeshGenerator
    {
        [SerializeField] private Vector3[] vs = new Vector3[4];
        protected override void SetMeshNums()
        {
            numVertices = 4;
            numTriangles = 12;
        }

        protected override void SetVertices()
        {
            vertices.AddRange(vs);
        }

        protected override void SetTriangles()
        {
            triangles.Add(0);
            triangles.Add(1);
            triangles.Add(2);

            triangles.Add(0);
            triangles.Add(2);
            triangles.Add(3);

            triangles.Add(2);
            triangles.Add(1);
            triangles.Add(3);

            triangles.Add(1);
            triangles.Add(0);
            triangles.Add(3);
        }
    }
}
