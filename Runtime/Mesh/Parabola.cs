using System;
using UnityEngine;

namespace CommonBase
{
    [RequireComponent(typeof(LineRenderer))]
    public class Parabola
    {
        /*Eqation
           x^2 = 4p(y-height) :p<0 
            => draw between 
            pos0(sqrt(-4p*height):a, 0)~ pos1(b:b<0,c)
        */
        public static Vector3[] DrawSymmetryParabola(int resolution, float height, Vector3 target0, Vector3 target1)
        {
            Vector3 pos0 = target0;
            Vector3 pos1 = target1;

            float deltaX = (pos1.x - pos0.x) / resolution;
            float deltaZ = (pos1.z - pos0.z) / resolution;

            float p, a, b, c = pos1.y - pos0.y;
            float h = c <= height ? height : c;

            if (c == 0)
            {
                a = (pos0.x - pos1.x) * 0.5f;
                b = -a;

                p = a * a / (-4 * h);
                if (p == 0)
                {
                    return StraightLine(target0, target1);
                }
            }
            else
            {
                p = pos0.x - pos1.x;
                p /= Mathf.Sqrt(4 * h) + Mathf.Sqrt(4 * (h - c));
                p *= -p;
                if (p == 0)
                {
                    return StraightLine(target0, target1);
                }

                a = deltaX >= 0 ? -Mathf.Sqrt(4 * p * -h) : Mathf.Sqrt(4 * p * -h);
                b = deltaX >= 0 ? Mathf.Sqrt(4 * p * (c - h)) : -Mathf.Sqrt(4 * p * (c - h));
            }

            Vector3[] vertices = new Vector3[resolution + 1];
            //setVertices
            vertices[0] = pos0;
            vertices[resolution] = pos1;
            for (int i = 1; i < resolution; i++)
            {
                float x = a + (deltaX * i);
                float y = x * x / (4 * p) + h;
                vertices[i].x = pos0.x + x - a;
                vertices[i].z = pos0.z + (deltaZ * i);
                vertices[i].y = pos0.y + y;
            }

            return vertices;
            ////setLine
            //line.positionCount = resolution + 1;
            //line.SetPositions(vertices);
        }

        public static Vector3[] DrawGravityParabola(Vector3 p1, Vector3 p2, float t, float expectationSplitLength)
        {
            var g = 9.8f;
            Vector3 v0 = new Vector3((p2.x - p1.x) / t, (float)(p2.y - p1.y + .5 * g * t * t) / t, (p2.z - p1.z) / t);
            var p = MathF.Sqrt(v0.x * v0.x + v0.z * v0.z);
            var c = -MathF.Pow(p, 2) / g;
            var arcLength = GetGravityParabolaArcLength(c, v0.y / p,
                v0.y / p - (g / (p * p) * (Vector3.Distance(new Vector3(p1.x, 0, p1.z), new Vector3(p2.x, 0, p2.z)))));
            var resolution = (int)(arcLength / expectationSplitLength);
            Debug.Log("采样点 线段垂直长度:" + Vector3.Distance(p1, p2));
            Debug.Log("采样点 线段长度:" + arcLength);
            Debug.Log("采样点 v0:" + v0);
            Debug.Log("采样点 t:" + t);
            Debug.Log("采样点 固定距离:" + expectationSplitLength);
            Debug.Log("采样点:" + resolution + "个");
            var ct = 0f;

            Vector3[] vertices = new Vector3[resolution + 1];
            var deltaTime = t / resolution;
            //setVertices
            vertices[0] = p1;
            vertices[resolution] = p2;
            for (int i = 1; i < resolution; i++)
            {
                ct += deltaTime;
                vertices[i] = new Vector3(p1.x + v0.x * ct, p1.y + v0.y * ct - .5f * g * ct * ct, p1.z + v0.z * ct);
            }

            return vertices;

            //var g = 9.8f;
            //Vector3 v0 = new Vector3((p2.x - p1.x) / t, (float)(p2.y - p1.y + .5 * g * t * t) / t, (p2.z - p1.z) / t);
            //var p = MathF.Sqrt(v0.x * v0.x + v0.z * v0.z);
            //var c = -MathF.Pow(p, 3) / g;
            ////var arcLength = GetGravityParabolaArcLength(c, 0, Vector3.Distance(p2, p1));
            //var resolution = (int)(t / Time.fixedDeltaTime * Vector3.Distance(p1, p2) / 1000);
            //Debug.Log("采样点 线段垂直长度:" + Vector3.Distance(p1, p2));
            ////Debug.Log("采样点 线段长度:" + t/Time.deltaTime);
            //Debug.Log("采样点 v0:" + v0);
            //Debug.Log("采样点 t:" + t);
            //Debug.Log("采样点 固定距离:" + expectationSplitLength);
            //Debug.Log("采样点:" + resolution + "个");
            //var ct = 0f;

            //Vector3[] vertices = new Vector3[resolution + 1];
            //var deltaTime = t / resolution;
            ////setVertices
            //vertices[0] = p1;
            //vertices[resolution] = p2;
            //for (int i = 1; i < resolution; i++)
            //{
            //    ct += deltaTime;
            //    vertices[i] = new Vector3(p1.x + v0.x * ct, p1.y + v0.y * ct - .5f * g * ct * ct, p1.z + v0.z * ct);
            //}

            //return vertices;
        }

        public static float GetGravityParabolaArcLength(float p, float start, float end)
        {
            return p * (GetDefiniteIntegrals(end) - GetDefiniteIntegrals(start));
        }

        public static float GetDefiniteIntegrals(float u)
        {
            return (.5f * u * Mathf.Sqrt(1 + u * u) + .5f * Mathf.Log((u + Mathf.Sqrt(u * u + 1))));
        }


        static Vector3[] StraightLine(Vector3 target0, Vector3 target1)
        {
            Vector3[] vectors = new Vector3[2];
            vectors[0] = target0;
            vectors[1] = target1;
            return vectors;
        }
    }
}

