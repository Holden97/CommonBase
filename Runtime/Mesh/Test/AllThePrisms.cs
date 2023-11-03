using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CommonBase
{
    public class AllThePrisms : AbstractMonoMeshGenerator
    {
        [SerializeField, Range(3, 20)] private int numSides = 3;
        [SerializeField] private float frontRadius;
        [SerializeField] private float backRadius;
        [SerializeField] private float length;

        [SerializeField] private Gradient gradient;
        protected override void SetMeshNums()
        {
            numVertices = 6 * numSides;
            numTriangles = 12 * (numSides - 1);
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

            for (int i = 1; i <= numSides; i++)
            {
                int val = numSides + 4 * (i - 1);

                triangles.Add(val);
                triangles.Add(val + 1);
                triangles.Add(val + 2);

                triangles.Add(val);
                triangles.Add(val + 3);
                triangles.Add(val + 1);
            }

            for (int i = 1; i < numSides - 1; i++)
            {
                triangles.Add(5 * numSides);
                triangles.Add(5 * numSides + i);
                triangles.Add(5 * numSides + i + 1);
            }
        }

        protected override void SetUVs()
        {
        }

        protected override void SetVertexColours()
        {
            for (int i = 0; i < numVertices; i++)
            {
                vertexColours.Add(gradient.Evaluate((float)i / numVertices));
            }
        }

        protected override void SetVertices()
        {
            Vector3[] vs = new Vector3[2 * numSides];
            for (int i = 0; i < numSides; i++)
            {
                float angle = 2 * Mathf.PI * i / numSides;
                vs[i] = new Vector3(frontRadius * Mathf.Cos(angle), frontRadius * Mathf.Sin(angle), 0);
                vs[i + numSides] = new Vector3(backRadius * Mathf.Cos(angle), backRadius * Mathf.Sin(angle), length);
            }

            for (int i = 0; i < numSides; i++)
            {
                vertices.Add(vs[i]);
            }

            for (int i = 0; i < numSides; i++)
            {
                vertices.Add(vs[i]);
                int secondIndex = i == 0 ? 2 * numSides - 1 : numSides + i - 1;
                vertices.Add(vs[secondIndex]);

                int thirdIndex = i == 0 ? numSides - 1 : i - 1;
                vertices.Add(vs[thirdIndex]);
                vertices.Add(vs[i + numSides]);
            }

            for (int i = 0; i < numSides; i++)
            {
                vertices.Add(vs[i + numSides]);
            }
        }
    }
}
