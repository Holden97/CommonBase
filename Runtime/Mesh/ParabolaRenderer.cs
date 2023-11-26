using System.Collections;
using System.Collections.Generic;
using System.Runtime.Remoting.Messaging;
using UnityEngine;

namespace CommonBase
{
    [RequireComponent(typeof(LineRenderer))]
    public class ParabolaRenderer : MonoBehaviour
    {
        public bool needLineRendereer;
        [Range(3, 10)] public float duration;
        [Range(0.5f, 10)] public float expectationSplitLength;

        public Transform start;
        private Vector3 end;

        public GameObject head;
        public GameObject body;

        private GameObject headGo;
        private Camera mainCam;
        private RaycastHit[] hits;
        private LineRenderer line;

        private void Awake()
        {
            ObjectPoolManager.Instance.CreatePool(50, body, "ParabolaRenderer");
            headGo = Instantiate(head);
            mainCam = Camera.main;
            hits = new RaycastHit[10];
            line = GetComponent<LineRenderer>();
        }

        private void Update()
        {
            line.enabled = needLineRendereer;
            ObjectPoolManager.Instance.PutbackAll("ParabolaRenderer");
            Physics.RaycastNonAlloc(mainCam.ScreenPointToRay(Input.mousePosition), hits, float.MaxValue);
            foreach (var hit in hits)
            {
                if (hit.transform != null && hit.transform.tag == "Ground")
                {
                    end = hit.point;
                    break;
                }
            }
            if (end == null)
            {
                return;
            }
            var vertices = Parabola.DrawGravityParabola(start.position, end, duration, expectationSplitLength);

            line.positionCount = vertices.Length;
            for (int i = 0; i < vertices.Length; i++)
            {
                line.SetPosition(i, vertices[i]);
            }

            if (vertices.IsNullOrEmpty() || vertices.Length < 2)
            {
                return;
            }

            for (int i = 0; i < vertices.Length - 1; i++)
            {
                Vector3 vertice = vertices[i];
                var go = ObjectPoolManager.Instance.GetNextObject("ParabolaRenderer");
                go.transform.transform.position = vertice;
                go.transform.rotation = Quaternion.LookRotation(vertices[i + 1] - vertices[i]);
            }

            headGo.transform.position = vertices[vertices.Length - 1];
            headGo.transform.rotation = Quaternion.LookRotation(vertices[vertices.Length - 1] - vertices[vertices.Length - 2]);
        }
    }
}
