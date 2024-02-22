using CommonBase;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace CommonBase
{
    public static class MeshUtil
    {
        private static Dictionary<string, Mesh> meshDictionary = new Dictionary<string, Mesh>();
        public static Mesh Quad(Vector3 pos)
        {
            Mesh mesh;
            var key = $"MeshUtil_Quad_{pos}";
            if (meshDictionary.ContainsKey(key))
            {
                mesh = meshDictionary[key];
            }
            else
            {
                mesh = CreateNewMesh(pos);
                meshDictionary.Add(key, mesh);
            }

            return mesh;
        }

        public static Mesh PlantZone(Vector2Int[] points)
        {
            Mesh mesh = new Mesh();
            var verticesList = new List<Vector3>();
            var trianglesList = new List<int>();
            Vector3[] vecs = new Vector3[4]
            {
            new Vector3(0.0f, 0.0f, 0.0f),
            new Vector3(0.0f, 1.0f, 0.0f),
            new Vector3(1.0f, 0.0f, 0.0f),
            new Vector3(1.0f, 1.0f, 0.0f),
        };
            int[] triIndex = new int[6]{
                    0,1,3,0,3,2,
            };

            var meshVerticesCount = 4 * points.Length;
            for (int i = 0; i < meshVerticesCount; i++)
            {
                var offset = points[i / 4].To3().ToFloat();
                var verBase = vecs[i % 4];
                verticesList.Add(verBase + offset);
            }
            for (int i = 0; i < 6 * points.Length; i++)
            {
                trianglesList.Add(triIndex[i % 6] + 4 * (i / 6));
            }
            mesh.vertices = verticesList.ToArray();
            mesh.triangles = trianglesList.ToArray();
            return mesh;
        }

        private static Mesh CreateNewMesh(Vector3 pos)
        {
            Mesh mesh;
            {
                mesh = new Mesh();
                mesh.vertices = new Vector3[4]
        {
                new Vector3(0.0f, 0.0f, 0.0f)+pos,
                new Vector3(0.0f, 1.0f, 0.0f)+pos,
                new Vector3(1.0f, 0.0f, 0.0f)+pos,
                new Vector3(1.0f, 1.0f, 0.0f)+pos,
        };
                mesh.triangles = new int[6]
                {
                0,1,3,
                0,3,2,
                };
                mesh.RecalculateBounds();
                mesh.RecalculateNormals();
            }

            return mesh;
        }

        public static Mesh GreenZoom(Vector3 pos)
        {
            Mesh mesh;
            var key = $"MeshUtil_Quad_Green_{pos}";
            if (meshDictionary.ContainsKey(key))
            {
                mesh = meshDictionary[key];
            }
            else
            {
                mesh = CreateNewMesh(pos);
                mesh.uv = new Vector2[]
                {
                new Vector2(0.2f,0),
                new Vector2(0.4f,0),
                new Vector2(0.2f,1f),
                new Vector2(0.4f,1f),
                };
                meshDictionary.Add(key, mesh);
            }
            return mesh;
        }

        public static bool InFan(this Vector3 point, Fan fan)
        {
            return fan.PointInRange(point);
        }

        public static void DrawFanMeshLine(this GameObject gameObject, Mesh mesh, float duration = 5)
        {
            var lineRenderer = gameObject.AddComponent<LineRenderer>();
            for (int i = 0; i < mesh.vertices.Length; i++)
            {
                lineRenderer.SetPosition(i, mesh.vertices[i]);
            }
            new BaseTimer(duration, onComplete: () =>
            {
                GameObject.Destroy(lineRenderer);
            });
        }
    }
}
