//使用utf-8
using UnityEngine;
using UnityEngine.UI;

namespace CommonBase
{
    /// <summary>
    /// 鼠标矩形选择器
    /// </summary>
    public class PointerRectSelector : MonoSingleton<PointerRectSelector>
    {
        private Vector3 onClickLeftStartPosition;
        private Vector3 onClickLeftEndPosition;

        private Vector3 onClickLeftStartPositionWorldPosition;
        private Vector3 onClickLeftEndPositionWorldPosition;

        public Rect screenRealSelection = new Rect();
        public Vector3 CurrentPointerPos => onClickLeftEndPositionWorldPosition;

        public Rect WorldRectXY => InputUtils.GetWorldRectXY(onClickLeftStartPositionWorldPosition, onClickLeftEndPositionWorldPosition);

        private Vector2 lowerLeft;
        private Vector2 upperRight;

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
            onClickLeftStartPosition = InputUtils.GetMousePosition();
            onClickLeftStartPositionWorldPosition = Camera.main.ScreenToWorldPoint(onClickLeftStartPosition);
            SelectedArea.GetComponent<Image>().enabled = true;
        }

        public void OnUp()
        {
            onClickLeftEndPosition = InputUtils.GetMousePosition();
            onClickLeftEndPositionWorldPosition = Camera.main.ScreenToWorldPoint(onClickLeftEndPosition);
            SelectedArea.GetComponent<Image>().enabled = false;
        }

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

        public void OnHolding()
        {
            onClickLeftEndPosition = InputUtils.GetMousePosition();
            onClickLeftEndPositionWorldPosition = Camera.main.ScreenToWorldPoint(onClickLeftEndPosition);
            lowerLeft = new Vector2(Mathf.Min(onClickLeftStartPosition.x, onClickLeftEndPosition.x), Mathf.Min(onClickLeftStartPosition.y, onClickLeftEndPosition.y));
            upperRight = new Vector2(Mathf.Max(onClickLeftStartPosition.x, onClickLeftEndPosition.x), Mathf.Max(onClickLeftStartPosition.y, onClickLeftEndPosition.y));
            screenRealSelection.position = lowerLeft;
            screenRealSelection.size = upperRight - lowerLeft;
        }
    }
}