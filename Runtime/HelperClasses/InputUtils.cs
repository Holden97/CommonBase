using System.Collections.Generic;
using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace CommonBase
{
    public static class InputUtils
    {
        public static Vector3 GetMouseWorldPosition()
        {
#if ENABLE_INPUT_SYSTEM
            Vector2 mousePosition = Mouse.current.position.ReadValue();
#else
            Vector2 mousePosition=Input.mousePosition;
#endif
            var result = Camera.main.ScreenToWorldPoint(mousePosition);
            Debug.Log($"GetMouseWorldPosition:{result}");
            return result;
        }

        public static Vector3 GetMouseWorldPositionFixedZ(float z = 0)
        {
#if ENABLE_INPUT_SYSTEM
            Vector2 mousePosition = Mouse.current.position.ReadValue();
#else
            Vector2 mousePosition=Input.mousePosition;
#endif
            var result = Camera.main.ScreenToWorldPoint(mousePosition);
            Debug.Log($"GetMouseWorldPosition:{result}");
            return new Vector3(result.x, result.y, z);
        }

        public static Vector3 GetMousePosition()
        {
#if ENABLE_INPUT_SYSTEM
            Vector2 mousePosition = Mouse.current.position.ReadValue();
#else
            Vector2 mousePosition=Input.mousePosition;
#endif
            //Debug.Log($"dirmousePosition:{mousePosition}");
            return mousePosition;
        }

        public static Vector3 GetWorldPosition(this Vector3 screenPos)
        {
            if (Camera.main == null)
            {
                Debug.LogError("Main Camera is null!");
                return Vector3.zero;
            }
            Vector2 worldPoint = Camera.main.ScreenToWorldPoint(screenPos);
            return worldPoint;
        }

        public static Vector3 GetWorldPosition(this Vector3Int screenPos)
        {
            var vec3 = new Vector3(screenPos.x, screenPos.y, screenPos.z);
            return GetWorldPosition(vec3);
        }

        public static Vector3 GetScreenPosition(this Vector3 worldPos)
        {
            if (Camera.main == null)
            {
                Debug.LogError("Main Camera is null!");
                return Vector3.zero;
            }
            return Camera.main.WorldToScreenPoint(worldPos);
        }

        public static Vector3 GetMousePositionToWorldWithSameZ(this Vector3 a)
        {
#if ENABLE_INPUT_SYSTEM
            Vector2 mousePosition = Mouse.current.position.ReadValue();
#else
            Vector2 mousePosition=Input.mousePosition;
#endif
            var result = Camera.main.ScreenToWorldPoint(mousePosition);
            Debug.Log($"GetMouseWorldPosition:{result}");
            return new Vector3(result.x, result.y, a.z);
        }

        public static Vector3 GetMousePositionToWorldWithSpecificZ(float z)
        {
#if ENABLE_INPUT_SYSTEM
            Vector2 mousePosition = Mouse.current.position.ReadValue();
#else
            Vector2 mousePosition=Input.mousePosition;
#endif
            var result = Camera.main.ScreenToWorldPoint(mousePosition);
            Debug.Log($"GetMouseWorldPosition:{result}");
            return new Vector3(result.x, result.y, z);
        }

        public static List<Vector3> GetLinearDestinations(Vector3 originalDestination, int count, float interval)
        {
            var result = new List<Vector3>();
            for (int i = 0; i < count; i++)
            {
                result.Add(new Vector3(originalDestination.x + interval * i, originalDestination.y, originalDestination.z));
            }
            return result;
        }

        public static bool RectangleContainsScreenPoint(this RectTransform rect, Vector2 screenPoint)
        {
            return RectTransformUtility.RectangleContainsScreenPoint(rect, screenPoint);
        }

        public static Rect GetWorldRect(Vector3 startWorldPos, Vector3 endWorldPos)
        {
            Rect worldRect = new Rect();

            var lowerLeft = new Vector2(Mathf.Min(startWorldPos.x, endWorldPos.x), Mathf.Min(startWorldPos.y, endWorldPos.y));
            var upperRight = new Vector2(Mathf.Max(startWorldPos.x, endWorldPos.x), Mathf.Max(startWorldPos.y, endWorldPos.y));

            worldRect.position = lowerLeft;
            worldRect.size = upperRight - lowerLeft;

            return worldRect;
        }
    }
}
