﻿//使用utf-8
using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif
using UnityEngine.UI;

namespace CommonBase
{
    /// <summary>
    /// 鼠标矩形选择器
    /// </summary>
    public class PointerRectSelector : MonoSingleton<PointerRectSelector>
    {
        public RectDetetionType rectDetetionType;
        /// <summary>
        /// 使用HitCollider方式时使用
        /// </summary>
        public string colliderTag;

        private Vector3 startScreenPosition;
        private Vector3 endScreenPosition;

        private Vector3 startWorldPosition;
        private Vector3 endWorldPosition;

        private Rect screenRealSelection = new Rect();
        public Vector3 CurrentPointerPos => endWorldPosition;

        public Rect WorldRectXY => InputUtils.GetWorldRectXY(startWorldPosition, endWorldPosition);
        public Rect WorldRectXZ => InputUtils.GetWorldRectXZ(startWorldPosition, endWorldPosition);

        private Vector2 lowerLeft;
        private Vector2 upperRight;

        public Rect SelectionRect => screenRealSelection;

        private RectTransform selectedArea;
        [HideInInspector]
        public RectTransform SelectedArea
        {
            get
            {
                if (selectedArea == null)
                {
                    var selectCanvas = GameObject.Instantiate(Resources.Load<GameObject>("SelectCanvas"), null);
                    selectedArea = selectCanvas.transform.Find("SelectionArea").GetComponent<RectTransform>();
                }
                return selectedArea;
            }
        }

        public void OnDown()
        {
            startScreenPosition = InputUtils.GetMousePosition();
            startWorldPosition = GetWorldPoint(rectDetetionType, startScreenPosition);
            SelectedArea.GetComponent<Image>().enabled = true;
        }

        private static bool CheckGroundPosition(Camera camera, string colliderTag, out RaycastHit hit)
        {
            RaycastHit[] hits = new RaycastHit[10];
            Physics.RaycastNonAlloc(camera.ScreenPointToRay(InputUtils.GetMousePosition()), hits);
            hit = default;

            foreach (var h in hits)
            {
                if (h.collider != null && h.collider.tag == colliderTag)
                {
                    hit = h;
                    return true;
                }
            }
            return false;
        }

        public void OnUp()
        {
            endScreenPosition = InputUtils.GetMousePosition();
            endWorldPosition = GetWorldPoint(rectDetetionType, endScreenPosition);
            SelectedArea.GetComponent<Image>().enabled = false;
        }

        private Vector3 GetWorldPoint(RectDetetionType rectDetetionType, Vector3 screenPos)
        {
            Vector3 resutl = Vector3.zero;
            switch (rectDetetionType)
            {
                case RectDetetionType.HIT_COLLIDER:
                    if (CheckGroundPosition(Camera.main, colliderTag, out var raycastHit))
                    {
                        resutl = raycastHit.point;
                    }
                    break;
                case RectDetetionType.SCREEN_TO_WORLD:
                    resutl = Camera.main.ScreenToWorldPoint(screenPos);
                    break;
                default:
                    break;
            }
            return resutl;
        }

        /// <summary>
        /// 绘制效果
        /// </summary>
        public void Render()
        {
            RenderSelectionArea(lowerLeft, upperRight, selectedArea);
        }

        private void RenderSelectionArea(Vector2 lowerLeft, Vector2 upperRight, RectTransform selectRect)
        {
            var selectCanvas = selectRect.parent.transform;
            SelectedArea.position = lowerLeft;
            var originalVec2 = (upperRight - lowerLeft);
            var sX = originalVec2.x / Screen.width * selectCanvas.GetComponent<RectTransform>().rect.size.x;
            var sY = originalVec2.y / Screen.height * selectCanvas.GetComponent<RectTransform>().rect.size.y;
            SelectedArea.sizeDelta = new Vector2(sX, sY);
        }

        /// <summary>
        /// 更新信息
        /// </summary>
        public void OnHolding()
        {
            endScreenPosition = InputUtils.GetMousePosition();
            endWorldPosition = GetWorldPoint(rectDetetionType, endScreenPosition);
            lowerLeft = new Vector2(Mathf.Min(startScreenPosition.x, endScreenPosition.x), Mathf.Min(startScreenPosition.y, endScreenPosition.y));
            upperRight = new Vector2(Mathf.Max(startScreenPosition.x, endScreenPosition.x), Mathf.Max(startScreenPosition.y, endScreenPosition.y));
            screenRealSelection.position = lowerLeft;
            screenRealSelection.size = upperRight - lowerLeft;
        }

        public enum RectDetetionType
        {
            /// <summary>
            /// 从屏幕发射射线与指定tag的Collider相交，形成世界空间中的判定矩形
            /// </summary>
            HIT_COLLIDER,
            /// <summary>
            /// 将屏幕坐标直接转换为空间坐标，形成世界空间中的判定矩形
            /// </summary>
            SCREEN_TO_WORLD,
        }
    }
}