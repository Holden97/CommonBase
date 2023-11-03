using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CommonBase
{
    public class AllThePolygons : AbstractMonoMeshGenerator
    {
        [SerializeField, Range(3, 20)] private int numSides = 3;
        [SerializeField] private float radius;
        protected override void SetMeshNums()
        {
            numVertices = numSides;
            numTriangles = 3 * (numSides - 2);
        }

        protected override void SetNormals()
        {
        }

        protected override void SetTangents()
        {
        }

        protected override void SetTriangles()
        {
            for (int i = 1; i < numSides - 1; i++)
            {
                triangles.Add(0);
                triangles.Add(i + 1);
                triangles.Add(i);
            }
        }

        protected override void SetUVs()
        {
            for (int i = 0; i < numSides; i++)
            {
                float uvX = vertices[i].x * 2;
                float uvY = vertices[i].y * 2;
                Vector2 uv = new Vector2(uvX, uvY);

                uvs.Add(uv);
            }
        }

        protected override void SetVertexColours()
        {
        }

        protected override void SetVertices()
        {
            for (int i = 0; i < numSides; i++)
            {
                float angle = 2 * Mathf.PI * i / numSides;
                vertices.Add(new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0));
            }
        }
    }
}
