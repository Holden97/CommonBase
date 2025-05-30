using UnityEngine;
using UnityEditor;
using UnityEngine.Tilemaps;

namespace CommonBase
{
    [InitializeOnLoad]
    public class TilemapCoordinateDisplay
    {
        private static Tilemap activeTilemap;
        static Grid grid = null;
        static Vector3Int cellPosition = default;
        static TilemapCoordinateDisplay()
        {
            SceneView.duringSceneGui += OnSceneGUI;
        }

        static void OnSceneGUI(SceneView sceneView)
        {
            Event currentEvent = Event.current;
            GameObject selectedObject = Selection.activeGameObject;
            if (currentEvent.type == EventType.MouseMove || currentEvent.type == EventType.MouseDown)
            {
                Vector2 mouseWorldPosition = HandleUtility.GUIPointToWorldRay(currentEvent.mousePosition).origin;

                // Assuming you have a Tilemap component assigned or you can find it by tag, layer, etc.
                // Tilemap tilemap = GameObject.FindObjectOfType<Tilemap>();
                if (selectedObject != null)
                {
                    // 优先从选中对象获取 Tilemap 组件
                    if (selectedObject.TryGetComponent<Tilemap>(out activeTilemap))
                    {
                        // 若选中对象有 Tilemap 组件，获取其关联的 Grid 组件
                        grid = activeTilemap.layoutGrid;
                    }
                    else if (selectedObject.TryGetComponent<Grid>(out grid))
                    {
                        // 若选中对象只有 Grid 组件，尝试获取其下第一个激活的 Tilemap
                        activeTilemap = selectedObject.GetComponentInChildren<Tilemap>(true);
                    }
                }
                if (activeTilemap != null)
                {
                    cellPosition = activeTilemap.WorldToCell(mouseWorldPosition);
                    Debug.Log("Mouse Tilemap Position: " + cellPosition);
                }
            }

            // 在 Scene 视图中绘制坐标文本
            Handles.BeginGUI();
            // 使用括号包裹条件表达式，避免冒号提前结束插值
            GUI.Label(new Rect(10, 10, 300, 20), $"当前活跃 Tilemap: {(activeTilemap != null ? activeTilemap.name : null)}, Tile 坐标: {cellPosition}");
            Handles.EndGUI();

            // // 可选：在世界中标出当前格子位置
            // Handles.color = Color.red;
            // Handles.DrawWireCube(grid.CellToWorld(cellPosition) + grid.cellSize / 2, grid.cellSize);
        }
    }
}
