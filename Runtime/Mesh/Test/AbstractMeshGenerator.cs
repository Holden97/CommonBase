using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.Rendering;
using UnityEngine;

namespace CommonBase
{
    [RequireComponent(typeof(MeshFilter))]
    [RequireComponent(typeof(MeshRenderer))]
    [RequireComponent(typeof(MeshCollider))]
    [ExecuteInEditMode]
    public abstract class AbstractMeshGenerator : MonoBehaviour
    {
        [SerializeField]
        protected Material material;

        protected List<Vector3> vertices;
        protected List<int> triangles;

        protected MeshFilter meshFilter;
        protected MeshRenderer meshRenderer;
        protected MeshCollider meshCollider;
        protected Mesh mesh;

        protected int numVertices;
        protected int numTriangles;

        protected List<Vector3> normals;
        protected List<Vector4> tangents;
        protected List<Vector2> uvs;
        protected List<Color32> vertexColours;

        private void Update()
        {
            meshFilter = GetComponent<MeshFilter>();
            meshRenderer = GetComponent<MeshRenderer>();
            meshCollider = GetComponent<MeshCollider>();

            meshRenderer.material = material;

            InitMesh();
            SetMeshNums();

            CreateMesh();
        }

        protected abstract void SetMeshNums();

        private bool ValidateMesh()
        {
            string errorStr = "";

            errorStr += vertices.Count == numVertices ? "" : "Should be " + numVertices + " vertices,but there are " + vertices.Count + ".";
            errorStr += triangles.Count == numTriangles ? "" : "Should be " + numTriangles + " triangles,but there are " + triangles.Count + ".";

            errorStr += normals.Count == numVertices || normals.Count == 0 ? "" : "Should be " + numVertices + " normals,but there are " + normals.Count + ".";
            errorStr += tangents.Count == numVertices || tangents.Count == 0 ? "" : "Should be " + numVertices + " tangents,but there are " + tangents.Count + ".";
            errorStr += uvs.Count == numVertices || uvs.Count == 0 ? "" : "Should be " + numVertices + " uvs,but there are " + uvs.Count + ".";
            errorStr += vertexColours.Count == numVertices || vertexColours.Count == 0 ? "" : "Should be " + numVertices + " vertexColours,but there are " + vertexColours.Count + ".";


            bool isValid = string.IsNullOrEmpty(errorStr);
            if (!isValid)
            {
                Debug.LogError("Not drawing mesh." + errorStr);
            }
            return isValid;
        }

        private void InitMesh()
        {
            vertices = new List<Vector3>();
            triangles = new List<int>();

            normals = new List<Vector3>();
            tangents = new List<Vector4>();
            uvs = new List<Vector2>();
            vertexColours = new List<Color32>();
        }

        private void CreateMesh()
        {
            mesh = new Mesh();
            SetVertices();
            SetTriangles();

            SetNormals();
            SetTangents();
            SetUVs();
            SetVertexColours();

            if (ValidateMesh())
            {
                mesh.SetVertices(vertices);
                mesh.SetTriangles(triangles, 0);

                if (normals.Count == 0)
                {
                    mesh.RecalculateNormals();
                    normals.AddRange(mesh.normals);
                }

                mesh.SetNormals(normals);
                mesh.SetTangents(tangents);
                mesh.SetUVs(0, uvs);
                mesh.SetColors(vertexColours);


                meshFilter.mesh = mesh;
                meshCollider.sharedMesh = mesh;
            }
        }

        protected abstract void SetVertices();
        protected abstract void SetTriangles();
        protected abstract void SetNormals();
        protected abstract void SetTangents();
        protected abstract void SetUVs();
        protected abstract void SetVertexColours();


    }
}
