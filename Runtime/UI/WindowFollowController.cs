using UnityEngine;

namespace CommonBase
{
    [RequireComponent(typeof(IFloatWindow))]
    public class WindowFollowController : MonoBehaviour
    {
        public Transform worldPosition;
        public RectTransform floatWindowTransform;

        private void Awake()
        {
            floatWindowTransform = GetComponent<IFloatWindow>().FloatWindowTransform as RectTransform;
        }

        private void LateUpdate()
        {
            if (worldPosition) floatWindowTransform.position = Camera.main!.WorldToScreenPoint(worldPosition.position);
        }

        public void SetWorldPosition(Vector3 position)
        {
            worldPosition.position = position;
        }
    }
}