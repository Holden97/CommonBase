using System;
using UnityEngine;

namespace CommonBase
{
    public static class VectorExtension
    {
        private static readonly Vector3 flipOneVectorX = new Vector3(-1f, 1f, 1f);
        /// <summary>
        /// Shorthand for writing Vector3(-1, 1, 1).
        /// </summary>
        public static Vector3 flipOneX => flipOneVectorX;

        private static readonly Vector3 flipOneVectorY = new Vector3(1f, -1f, 1f);
        /// <summary>
        /// Shorthand for writing Vector3(-1, 1, 1).
        /// </summary>
        public static Vector3 flipOneY => flipOneVectorY;
        public static Vector2Int undefinedV2Int = new Vector2Int(-1, -1);
        public static Vector3Int ToCell(this Vector3 vector)
        {
            return new Vector3Int(Mathf.FloorToInt(vector.x), Mathf.FloorToInt(vector.y), Mathf.FloorToInt(vector.z));
        }

        public static Vector3 ToCellCenter(this Vector3 vector)
        {
            return new Vector3(Mathf.FloorToInt(vector.x) + 0.5f, Mathf.FloorToInt(vector.y) + 0.5f, Mathf.FloorToInt(vector.z));
        }

        public static Vector3 ToCellBottom(this Vector3 vector)
        {
            return new Vector3(Mathf.FloorToInt(vector.x) + 0.5f, Mathf.FloorToInt(vector.y), Mathf.FloorToInt(vector.z));
        }

        public static Vector2 ToCellBottom(this Vector2Int vector)
        {
            return new Vector2(Mathf.FloorToInt(vector.x) + 0.5f, Mathf.FloorToInt(vector.y));
        }

        public static Vector2Int ToCell(this Vector2 vector)
        {
            return new Vector2Int(Mathf.FloorToInt(vector.x), Mathf.FloorToInt(vector.y));
        }

        public static Vector2Int ToCellRound(this Vector2 vector)
        {
            return new Vector2Int(Mathf.RoundToInt(vector.x), Mathf.RoundToInt(vector.y));
        }

        public static Vector3Int To3(this Vector2Int vector2)
        {
            return new Vector3Int(vector2.x, vector2.y, 0);
        }

        public static Vector3 ToFloat(this Vector3Int vector3)
        {
            return new Vector3(vector3.x, vector3.y, vector3.z);
        }

        public static Vector3 To3(this Vector2 vector2)
        {
            return new Vector3(vector2.x, vector2.y, 0);
        }

        public static Vector2Int To2(this Vector3Int vector3)
        {
            return new Vector2Int(vector3.x, vector3.y);
        }

        public static Vector2 ToWorldVector2(this Vector3Int worldVector)
        {
            return new Vector2(worldVector.x, worldVector.y);
        }

        public static Vector2Int ToWorldVector2Int(this Vector3Int worldVector)
        {
            return new Vector2Int(worldVector.x, worldVector.y);
        }

        public static bool InGrid(this Vector2 curPos, Vector2Int refPos)
        {
            return curPos.ToCell() == refPos;
        }

        public static Vector2 To2(this Vector3 worldVector)
        {
            return new Vector2(worldVector.x, worldVector.y);
        }

        public static bool InStraightLine(this Vector2Int thisPoint, Vector3Int refPoint)
        {
            return thisPoint.x == refPoint.x || thisPoint.y == refPoint.y;
        }

        public static bool InStraightLine(this Vector2Int thisPoint, Vector2Int refPoint)
        {
            return thisPoint.x == refPoint.x || thisPoint.y == refPoint.y;
        }

        /// <summary>
        /// 取x,y坐标和最小的点
        /// </summary>
        /// <param name="v"></param>
        /// <returns></returns>
        public static Vector2Int Min(this Vector2Int[] v)
        {
            if (v.Length <= 0)
                return default;
            Vector2Int min = v[0];
            for (int i = 0; i < v.Length; i++)
            {
                if (v[i].x + v[i].y < min.x + min.y)
                {
                    min = v[i];
                }
            }
            return min;
        }

        /// <summary>
        /// 取x,y坐标和最大的点
        /// </summary>
        /// <param name="v"></param>
        /// <returns></returns>
        public static Vector2Int Max(this Vector2Int[] v)
        {
            if (v.Length <= 0)
                return default;
            Vector2Int max = v[0];
            for (int i = 0; i < v.Length; i++)
            {
                if (v[i].x + v[i].y > max.x + max.y)
                {
                    max = v[i];
                }
            }
            return max;
        }

        public static Vector2 Rotate(this Vector2 originalVector, float angle)
        {
            // 将角度转换为弧度
            float rotationAngleRadians = angle * Mathf.Deg2Rad;

            // 计算旋转后的向量分量
            float rotatedX = originalVector.x * Mathf.Cos(rotationAngleRadians) - originalVector.y * Mathf.Sin(rotationAngleRadians);
            float rotatedY = originalVector.x * Mathf.Sin(rotationAngleRadians) + originalVector.y * Mathf.Cos(rotationAngleRadians);

            // 创建旋转后的向量
            return new Vector2(rotatedX, rotatedY);
        }

        public static Vector2 RotateByQuaternion(this Vector2 originalVector, float rotationAngleDegrees)
        {
            Quaternion rotation = Quaternion.Euler(0.0f, 0.0f, rotationAngleDegrees);
            return rotation * originalVector;
        }

        public static Vector2 Lerp(Vector2 a, Vector2 b, float amount)
        {
            return a + (b - a) * amount;
        }

        public static Vector2 GetRandomAdjacentVector(this Vector2 originalVector, float maxAngle)
        {
            // 在指定角度范围内生成随机弧度
            float randomAngleRad = UnityEngine.Random.Range(-maxAngle / 2, maxAngle / 2);

            // 使用Quaternion进行旋转
            Quaternion rotation = Quaternion.Euler(0, 0, randomAngleRad);
            Vector2 randomAdjacentVector = rotation * originalVector;

            return randomAdjacentVector;
        }

        public static Vector3 GetRandomAdjacentVector(this Vector3 originalVector, float maxAngle)
        {
            // 在指定角度范围内生成随机弧度
            float randomAngleRad = UnityEngine.Random.Range(-maxAngle / 2, maxAngle / 2);

            // 使用Quaternion进行旋转
            Quaternion rotation = Quaternion.Euler(0, 0, randomAngleRad);
            Vector2 randomAdjacentVector = rotation * originalVector;

            return randomAdjacentVector;
        }
    }
}

