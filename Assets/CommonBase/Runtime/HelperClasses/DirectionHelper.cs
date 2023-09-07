using UnityEngine;

namespace CommonBase
{
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
    }
}
