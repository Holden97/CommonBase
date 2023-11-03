﻿using System.Collections.Generic;
using UnityEngine;

namespace CommonBase
{
    [RequireComponent(typeof(MeshFilter))]
    [RequireComponent(typeof(MeshRenderer))]
    [RequireComponent(typeof(MeshCollider))]
    [ExecuteInEditMode]
    public abstract class AbstractMesh
    {
        protected Material material;

        public Vector3 position = Vector3.zero;
        public Quaternion quaternion;
        public Vector3 scale = Vector3.one;

        protected List<Vector3> vertices;
        protected List<int> triangles;

        protected Mesh mesh;

        protected int numVertices;
        protected int numTriangles;

        protected List<Vector3> normals;
        protected List<Vector4> tangents;
        protected List<Vector2> uvs;
        protected List<Color32> vertexColours;

        //public List

        protected AbstractMesh(Vector3 position, Quaternion quaternion, Vector3 scale, Material material)
        {
            this.position = position;
            this.quaternion = quaternion;
            this.scale = scale;
            this.material = material;
        }

        public void SetPostion(Vector3 position)
        {
            this.position = position;
        }

        public void Update()
        {
            Matrix4x4 matrix = Matrix4x4.TRS(position, quaternion, Vector3.one);
            InitMesh();
            SetMeshNums();
            CreateMesh();
            Graphics.DrawMesh(mesh, matrix, material, LayerMask.NameToLayer("Default"));
        }


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


                //meshFilter.mesh = mesh;
                //meshCollider.sharedMesh = mesh;
            }
        }

        protected virtual void SetVertices() { }
        protected virtual void SetTriangles() { }
        protected virtual void SetNormals() { }
        protected virtual void SetTangents() { }
        protected virtual void SetUVs() { }
        protected virtual void SetVertexColours() { }

        protected virtual void SetMeshNums() { }

    }
}
