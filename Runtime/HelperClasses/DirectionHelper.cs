using UnityEngine;

namespace CommonBase
{
    public enum DirectionEnum { Left, Right, Up, Down,Center }
    public class DirectionHelper
    {
        public static Vector2Int JudgeDir(Vector2 center, Vector2 point)
        {
            var offset = point - center;
            if ((offset.y >= 2 * offset.x && offset.x >= 0) || (offset.y >= -2 * offset.x && offset.x <= 0))
            {
                return Vector2Int.up;
            }
            else if ((offset.y <= 2 * offset.x && offset.x <= 0) || (offset.y <= -2 * offset.x && offset.x >= 0))
            {
                return Vector2Int.down;
            }
            else if ((offset.y < 2 * offset.x && offset.y >= 0) || (offset.y > -2 * offset.x && offset.y <= 0))
            {
                return Vector2Int.right;
            }
            else if ((offset.y > 2 * offset.x && offset.y <= 0) || (offset.y < -2 * offset.x && offset.y >= 0))
            {
                return Vector2Int.left;
            }
            else
            {
                return Vector2Int.zero;
            }
        }

        public static DirectionEnum JudgeDirByDegree(Vector2 center, Vector2 point)
        {
            var offset = point - center;
            var r = Mathf.Atan2(offset.y, offset.x);
            var a = r * Mathf.Rad2Deg;
            if (a < 45 && a >= -45) return DirectionEnum.Right;
            else if (a < -135 || a >= 135) return DirectionEnum.Left;
            else if (a >= 45 && a < 135) return DirectionEnum.Up;
            else if (a < -45 && a >= -135) return DirectionEnum.Down;
            return DirectionEnum.Center;
        }
    }
}
