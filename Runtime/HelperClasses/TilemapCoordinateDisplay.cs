using UnityEngine;
using UnityEditor;
using UnityEngine.Tilemaps;

namespace CommonBase
{
    [InitializeOnLoad]
    public class TilemapCoordinateDisplay
    {
        static TilemapCoordinateDisplay()
        {
            SceneView.duringSceneGui += OnSceneGUI;
        }

        static void OnSceneGUI(SceneView sceneView)
        {
            // 检查当前选中的 GameObject 是否包含 Grid 或 Tilemap 组件
            GameObject selectedObject = Selection.activeGameObject;
            if (selectedObject == null || (!selectedObject.TryGetComponent<Grid>(out _) && !selectedObject.TryGetComponent<Tilemap>(out _)))
            {
                return;
            }
            if (Camera.current == null) return;

            Event e = Event.current;
            if (e == null) return;

            // 获取鼠标位置并转换为世界坐标
            Vector2 mousePos = e.mousePosition;
            mousePos.y = SceneView.currentDrawingSceneView.camera.pixelHeight - mousePos.y; // 处理y轴反转

            // 检查鼠标位置是否在相机视口内
            Vector3 viewportPos = SceneView.currentDrawingSceneView.camera.ScreenToViewportPoint(mousePos);
            if (viewportPos.x < 0 || viewportPos.x > 1 || viewportPos.y < 0 || viewportPos.y > 1 || viewportPos.z < 0)
            {
                return;
            }

            Vector3 worldPos = SceneView.currentDrawingSceneView.camera.ScreenToWorldPoint(mousePos);
            worldPos.z = 0;

            // 获取Tile坐标
            Grid grid = GameObject.FindFirstObjectByType<Grid>();
            if (grid == null) return;

            Vector3Int cellPosition = grid.WorldToCell(worldPos);

            // 在Scene视图中绘制坐标文本
            Handles.BeginGUI();
            GUI.Label(new Rect(10, 10, 200, 20), $"Tile坐标: {cellPosition}");
            Handles.EndGUI();

            // 可选：在世界中标出当前格子位置
            Handles.color = Color.red;
            Handles.DrawWireCube(grid.CellToWorld(cellPosition) + grid.cellSize / 2, grid.cellSize);
        }
    }
}
