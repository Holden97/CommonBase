using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CommonBase
{
    [RequireComponent(typeof(MeshFilter))]
    public class MeshNormalDisplayer : MonoBehaviour
    {
        [SerializeField] private bool drawNormals;
        [SerializeField] private float normalLength = .5f;

        private void OnDrawGizmosSelected()
        {
            if (drawNormals)
            {
                Mesh mesh = GetComponent<MeshFilter>().sharedMesh;

                if (mesh != null)
                {
                    for (int i = 0; i < mesh.vertexCount; i++)
                    {
                        Vector3 vertex = transform.TransformPoint(mesh.vertices[i]);
                        Vector3 normal = transform.TransformPoint(mesh.normals[i]);

                        Gizmos.color = Color.blue;
                        Gizmos.DrawLine(vertex, vertex + normalLength * normal);
                    }
                }
            }
        }
    }
}
