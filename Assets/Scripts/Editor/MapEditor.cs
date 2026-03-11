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

        SerializedObject so;
        SerializedProperty propMapName;
        SerializedProperty propMapSize;
        SerializedProperty propBrushHeight;
        SerializedProperty propBrushSize;

        readonly Vector3 tileCenterOffset = new(0.5f, 0, 0.5f);

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
            Handles.zTest = CompareFunction.Always;

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

            GUILayout.BeginHorizontal();
            EditorGUILayout.PropertyField(propMapSize);
            if (GUILayout.Button("Reload", GUILayout.Width(60)))
            {
                mapInstance.ReloadMap();
            }
            GUILayout.EndHorizontal();

            int toolIndex = GUILayout.Toolbar(
                (int)SelectedTool,
                toolbarButtons,
                GUILayout.Height(24),
                GUILayout.Width(32 * toolbarButtons.Length));

            SwitchTool((Tool)toolIndex);

            if (SelectedTool != Tool.View)
            {
                EditorGUILayout.PropertyField(propBrushHeight);
                propBrushHeight.intValue = Mathf.Clamp(propBrushHeight.intValue, -4, 11);
                EditorGUILayout.PropertyField(propBrushSize);
                propBrushSize.intValue = Mathf.Clamp(propBrushSize.intValue, 1, 8);
            }

            so.ApplyModifiedProperties();
        }

        // Scene view interactions
        private void OnSceneGUI()
        {
            if (SelectedTool == Tool.View) 
                return;

            // Allows the use of my custom cursor in the scene view
            EditorGUIUtility.AddCursorRect(new Rect(0, 0, Screen.width, Screen.height), MouseCursor.CustomCursor);

            Event e = Event.current;

            int height = propBrushHeight.intValue;
            int brushSize = SelectedTool == Tool.Edit ? propBrushSize.intValue : 1;

            bool isRaycastSuccessful = RaycastMouseToWorld(e.mousePosition, height, out Vector3 mouseWorldPos);
            Vector2Int tileCoord = Vector2Int.FloorToInt(new Vector2(mouseWorldPos.x, mouseWorldPos.z));

            List<Vector2Int> hoveredTiles = GetHoveredTiles(mouseWorldPos, brushSize);
            foreach (Vector2Int coord in hoveredTiles)
            {
                DrawTileGizmo(coord);
            }

            switch (e.type)
            {
                // Refocus on scene view if the mouse is hovering it without having to click
                case EventType.MouseMove:
                    SceneView.FocusWindowIfItsOpen(typeof(SceneView));
                    Repaint();
                    break;

                case EventType.KeyDown:
                    HandleKeyDown(e.keyCode);
                    break;

                case EventType.KeyUp:
                    HandleKeyUp(e.keyCode);
                    break;

                case EventType.MouseDown or EventType.MouseDrag:
                    if (e.button != 0) 
                        return;

                    if (SelectedTool == Tool.Edit)
                    {
                        mapInstance.PlaceTiles(hoveredTiles, height);
                    }
                    if (SelectedTool == Tool.Pick && e.type == EventType.MouseDown)
                    {
                        if (!isRaycastSuccessful)
                            return;
                        propBrushHeight.intValue = mapInstance.GetCellHeight(tileCoord.x, tileCoord.y);
                        SwitchTool(Tool.Edit);
                    }
                    e.Use();
                    break;

                default:
                    break;
            }
            so.ApplyModifiedProperties();
        }

        private bool RaycastMouseToWorld(Vector3 mousePos, int height, out Vector3 mouseWorldPos)
        {
            Ray ray = HandleUtility.GUIPointToWorldRay(mousePos);

            // When editing, raycast to a plane at the brush height
            if (SelectedTool == Tool.Edit)
            {
                Vector3 planeInPoint = new(0, height, 0);
                Plane groundPlane = new(Vector3.up, planeInPoint);

                if (groundPlane.Raycast(ray, out float distance))
                {
                    mouseWorldPos = ray.GetPoint(distance);
                    return true;
                }
            }
            // When picking a tile's height, raycast to the mesh collider to get the ground tile the mouse is hovering
            else
            {
                if (Physics.Raycast(ray, out RaycastHit hit, 200, mapLayerMask))
                {
                    mouseWorldPos = hit.point;
                    return true;
                }
            }
            mouseWorldPos = Vector3.negativeInfinity;
            return false;
        }
        private List<Vector2Int> GetHoveredTiles(Vector3 mouseWorldPos, int brushSize)
        {
            if (brushSize == 1)
            {
                Vector2Int tileCoord = Vector2Int.FloorToInt(new Vector2(mouseWorldPos.x, mouseWorldPos.z));
                return new List<Vector2Int>() { tileCoord };
            }

            List<Vector2Int> tiles = new(brushSize * brushSize);

            int xMin = Mathf.RoundToInt(mouseWorldPos.x - brushSize / 2.0f);
            int zMin = Mathf.RoundToInt(mouseWorldPos.z - brushSize / 2.0f);

            for (int z = zMin; z < zMin + brushSize; z++)
            {
                for (int x = xMin; x < xMin + brushSize; x++)
                {
                    tiles.Add(new Vector2Int(x, z));
                }
            }
            return tiles;
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
                    new Vector3(coord.x, cellHeight / 2f, coord.y) + tileCenterOffset,
                    new Vector3(1, cellHeight, 1));
                return;
            }

            // In edit mode, draw a gizmo to preview the change in height when a tile would be placed
            // (white = no change, green = add, red = remove)
            if (brushHeight == cellHeight)
            {
                Handles.color = Color.white;
                Handles.DrawWireCube(
                    new Vector3(coord.x, brushHeight, coord.y) + tileCenterOffset,
                    new Vector3(1, 0, 1));
            }
            else
            {
                int dy = brushHeight - cellHeight;
                Handles.color = dy > 0 ? Color.green : Color.red;
                Handles.DrawWireCube(
                    new Vector3(coord.x, (brushHeight + cellHeight) / 2f, coord.y) + tileCenterOffset,
                    new Vector3(1, Mathf.Abs(dy), 1));
            }
        }

        private void HandleKeyDown(KeyCode key)
        {
            switch (key)
            {
                case KeyCode.LeftAlt or KeyCode.RightAlt:
                    SwitchTool(Tool.Pick);
                    break;

                case KeyCode.LeftBracket:
                    propBrushSize.intValue--;
                    break;

                case KeyCode.RightBracket:
                    propBrushSize.intValue++;
                    break;

                case KeyCode.Minus or KeyCode.KeypadMinus:
                    propBrushHeight.intValue--;
                    break;

                case KeyCode.Equals or KeyCode.KeypadPlus:
                    propBrushHeight.intValue++;
                    break;

                default:
                    break;
            }
        }

        private void HandleKeyUp(KeyCode key)
        {
            switch (key)
            { 
                case KeyCode.LeftAlt or KeyCode.RightAlt:
                    SwitchTool(Tool.Edit);
                    break;

                default:
                    break;
            }
        }

        private void SwitchTool(Tool tool)
        {
            if (tool == SelectedTool)
                return;

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
