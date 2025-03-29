using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Rendering;

namespace ProjectQuad
{
    [CustomEditor(typeof(Map))]
    public class MapEditor : Editor
    {
        Map mapInstance;
        LayerMask mapLayerMask;
        readonly int s_MapEditorHash = "MapEditor".GetHashCode();

        SerializedObject so;
        SerializedProperty propMapName;
        SerializedProperty propMapSize;
        SerializedProperty propBrushHeight;
        SerializedProperty propBrushSize;

        readonly Vector3 tileCenterOffset = new (0.5f, 0, 0.5f);

        GUIContent[] toolbarButtons;
        Texture2D hiddenCursor;
        Texture2D pickToolCursor;
        readonly Vector2 pickToolCursorHotspot = new(3, 20);

        public enum Tool
        {
            View = 0,
            Edit = 1,
            Pick = 2
        }
        public Tool SelectedTool = Tool.View;

        //Initialization
        private void OnEnable()
        {
            so = serializedObject;

            mapInstance = (Map)target;
            mapInstance.InitializeMap();

            propMapName = so.FindProperty("mapName");
            propMapSize = so.FindProperty("mapSize");
            propBrushHeight = so.FindProperty("brushHeight");
            propBrushSize = so.FindProperty("brushSize");

            toolbarButtons = new GUIContent[]
            {
                new((Texture)EditorGUIUtility.Load("View.png"), "View Mode"),
                new((Texture)EditorGUIUtility.Load("Edit.png"), "Edit Mode"),
                new((Texture)EditorGUIUtility.Load("Pick.png"), "Copy Height")
            };

            hiddenCursor = (Texture2D)EditorGUIUtility.Load("HiddenCursor.png");
            pickToolCursor = (Texture2D)EditorGUIUtility.Load("PickCursor.png");
            
            mapLayerMask = LayerMask.GetMask("Map");
        }

        // Customizing the inspector
        public override void OnInspectorGUI()
        {
            so.Update();
      
            EditorGUILayout.PropertyField(propMapName);
            EditorGUILayout.PropertyField(propMapSize);

            int toolIndex = GUILayout.Toolbar(
                (int)SelectedTool,
                toolbarButtons,
                GUILayout.Height(24),
                GUILayout.Width(32 * toolbarButtons.Length));

            SwitchTool((Tool)toolIndex);

            if (SelectedTool != Tool.View) 
            {
                EditorGUILayout.PropertyField(propBrushHeight);
                EditorGUILayout.PropertyField(propBrushSize);
            }
            so.ApplyModifiedProperties();
        }

        // Scene view interactions
        private void OnSceneGUI()
        {
            if (SelectedTool == Tool.View) return;

            // Allows the use of my custom cursor in the scene view
            EditorGUIUtility.AddCursorRect(new Rect(0, 0, Screen.width, Screen.height), MouseCursor.CustomCursor);

            Handles.zTest = CompareFunction.Always;
            Event e = Event.current;
            int controlId = GUIUtility.GetControlID(s_MapEditorHash, FocusType.Passive);

            Ray ray = HandleUtility.GUIPointToWorldRay(e.mousePosition);
            Vector3 worldMousePos = Vector2.zero;
            Vector3Int worldTileCoord = Vector3Int.zero;
            int height = propBrushHeight.intValue;

            // When editing, raycast to a plane at the brush height
            if (SelectedTool == Tool.Edit)
            {
                Vector3 planeInPoint = new(0, height, 0);
                Plane groundPlane = new(Vector3.up, planeInPoint);

                if (groundPlane.Raycast(ray, out float distance))
                {
                    worldMousePos = ray.GetPoint(distance);
                    worldTileCoord = Vector3Int.FloorToInt(worldMousePos);
                }
            }
            // When picking a tile's height, raycast to the mesh collider to get the ground tile the mouse is hovering
            else
            {
                if (Physics.Raycast(ray, out RaycastHit hit, 200, mapLayerMask))
                {
                    worldMousePos = hit.point;
                    worldTileCoord = Vector3Int.FloorToInt(worldMousePos);       
                }
            }

            List<Vector2Int> tiles = GetHoveredTiles(worldMousePos, propBrushSize.intValue);
            if (SelectedTool == Tool.Edit)
            {
                foreach (Vector2Int coord in tiles)
                {
                    DrawTileGizmo(coord);
                }
            }
            else
            {
                DrawTileGizmo(new Vector2Int(worldTileCoord.x, worldTileCoord.z));
            }

            Vector3Int tileCoord = Vector3Int.FloorToInt(worldTileCoord - mapInstance.transform.position);

            switch (e.type)
            {
                case EventType.Layout:
                    HandleUtility.AddDefaultControl(controlId);
                    break;

                // Refocus on scene view if the mouse is hovering it without having to click
                case EventType.MouseMove:
                    SceneView.FocusWindowIfItsOpen(typeof(SceneView));
                    Repaint();
                    break;

                // Hold ALT to enable the Pick Tool
                case EventType.KeyDown:
                    if (e.keyCode == KeyCode.LeftAlt || e.keyCode == KeyCode.RightAlt)
                    {
                        SwitchTool(Tool.Pick);
                    }
                    break;

                case EventType.KeyUp:
                    if (e.keyCode == KeyCode.LeftAlt || e.keyCode == KeyCode.RightAlt)
                    {
                        SwitchTool(Tool.Edit);
                    }
                    break;

                case EventType.MouseDown:
                case EventType.MouseDrag:
                    if (e.button != 0)
                        return;
                    if (SelectedTool == Tool.Edit)
                    {
                        foreach (Vector2Int coord in tiles)
                        {
                            PlaceTile(coord.x, coord.y, height);
                        }
                    }
                    // For some reason this check only works if the MouseDown case is defined, even if empty
                    if (SelectedTool == Tool.Pick && e.type == EventType.MouseDown)
                    {
                        SetBrushToTileHeight(tileCoord.x, tileCoord.z);
                        SwitchTool(Tool.Edit);
                    }
                    e.Use();
                    break;

                default:
                    break;
            }
        }

        private List<Vector2Int> GetHoveredTiles(Vector3 worldMousePos, int brushSize)
        {
            List<Vector2Int> tiles = new(brushSize * brushSize);

            int xMin = Mathf.RoundToInt(worldMousePos.x - brushSize / 2.0f);
            int zMin = Mathf.RoundToInt(worldMousePos.z - brushSize / 2.0f);

            for (int z = zMin; z < zMin + brushSize; z++)
            {
                for (int x = xMin; x < xMin + brushSize; x++)
                {
                    tiles.Add(new Vector2Int(x, z));
                }
            }
            return tiles;
        }

        private void PlaceTile(int x, int z, int height)
        {
            mapInstance.PlaceTile(x, z, height);
        }

        private void SetBrushToTileHeight(int x, int z)
        {
            propBrushHeight.intValue = mapInstance.GetCellHeight(x, z);
            so.ApplyModifiedProperties();
        }

        private void DrawTileGizmo(Vector2Int coord)
        {
            int brushHeight = propBrushHeight.intValue;
            int cellHeight = mapInstance.GetCellHeight(coord.x, coord.y);

            // In pick mode, draw a yellow square at the nearest cell's height to indicate which tile will be copied
            if (SelectedTool == Tool.Pick)
            {
                Handles.color = Color.yellow;
                Handles.DrawWireCube(
                    new Vector3(coord.x, cellHeight, coord.y) + tileCenterOffset,
                    new Vector3(1, 0, 1));
                return;
            }

            if (brushHeight == cellHeight)
            {
                Handles.DrawWireCube(
                    new Vector3(coord.x, brushHeight, coord.y) + tileCenterOffset,
                    new Vector3(1, 0, 1));
            }
            else
            {
                // In edit mode, draw a gizmo to preview the change in height when a tile would be placed
                // (white = no change, green = add, red = remove)
                int dy = brushHeight - cellHeight;
                Handles.color = dy > 0 ? Color.green : Color.red;
                Handles.DrawWireCube(
                    new Vector3(coord.x, (brushHeight + cellHeight) / 2f, coord.y) + tileCenterOffset,
                    new Vector3(1, Mathf.Abs(dy), 1));
            }

        }

        private void SwitchTool(Tool tool)
        {
            if (tool == SelectedTool) return;

            SelectedTool = tool;
            Repaint();
            SetCursor();
        }

        private void SetCursor()
        {
            switch (SelectedTool)
            {
                // Default cursor
                case Tool.View:
                    Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
                    break;
                // Hidden cursor
                case Tool.Edit:
                    Cursor.SetCursor(hiddenCursor, Vector2.zero, CursorMode.Auto);
                    break;
                // Eyedropper cursor
                case Tool.Pick:
                    Cursor.SetCursor(pickToolCursor, pickToolCursorHotspot, CursorMode.Auto);
                    break;
            }
        }
    }
}
