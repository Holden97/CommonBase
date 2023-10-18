using System;
using System.Collections.Generic;
using UnityEngine;

namespace CommonBase
{
    [RequireComponent(typeof(MeshFilter))]
    [RequireComponent(typeof(MeshRenderer))]
    [ExecuteInEditMode]
    public class AllTheTriangles : MonoBehaviour
    {
        [SerializeField]
        private Material material;
        [SerializeField]
        private Vector3[] vs = new Vector3[3];

        private List<Vector3> vertices;
        private List<int> triangles;

        public MeshFilter meshFilter;
        private MeshRenderer meshRenderer;
        private Mesh mesh;

        private void Update()
        {
            meshFilter = GetComponent<MeshFilter>();
            meshRenderer = GetComponent<MeshRenderer>();

            meshRenderer.material = material;

            vertices = new List<Vector3>();
            triangles = new List<int>();

            CreateMesh();
        }

        private void CreateMesh()
        {
            mesh = new Mesh();
            SetVertices();
            SetTriangles();

            mesh.SetVertices(vertices);
            mesh.SetTriangles(triangles, 0);

            mesh.RecalculateNormals();

            meshFilter.sharedMesh = mesh;
        }

        private void SetTriangles()
        {
            vertices.AddRange(vs);
        }

        private void SetVertices()
        {
            triangles.Add(0);
            triangles.Add(1);
            triangles.Add(2);
        }
    }
}
