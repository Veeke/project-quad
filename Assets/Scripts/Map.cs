using UnityEngine;

namespace ProjectQuad
{
    [ExecuteInEditMode]
    public class Map : MonoBehaviour
    {
        [SerializeField]
        string mapName;
        [SerializeField, Vector2IntRange(0, 0, 512, 512)]
        Vector2Int mapSize = new(32, 32);
        [SerializeField]
        Tileset tileset;

        [SerializeField, Range(-4, 12)]
        int brushHeight;

        HeightMap heightMap;

        [SerializeField]
        MapMeshGenerator meshGenerator;
        JsonDataService dataService = new();

        public void InitializeMap()
        {
            heightMap ??= LoadMapData();
        }

        public void EditMap(int x, int y, int height)
        {
            heightMap.SetCell(x, y, height);
            SaveMapData();
            meshGenerator.GenerateMapMesh(heightMap);
        }

        [ContextMenu("Reload Map")]
        public void ReloadMap()
        {
            heightMap = LoadMapData();
            tileset.RecalculateUVs();
            meshGenerator.GenerateMapMesh(heightMap);
        }

        public void ResetMap()
        {
            for (int y = 0; y < mapSize.y; y++)
            {
                for (int x = 0; x < mapSize.x; x++)
                {
                    heightMap.SetCell(x, y, 0);
                }
            }
            SaveMapData();
            meshGenerator.GenerateMapMesh(heightMap);
        }

        public int GetCellHeight(int x, int y)
        {
            return heightMap.GetCell(x, y);
        }

        [ContextMenu("Resize Map")]
        public void ResizeMap()
        {
            if (heightMap.GetMapSize() != mapSize)
            {
                heightMap.SetMapSize(mapSize);
                SaveMapData();
                ReloadMap();
            }
        }

        public void SaveMapData()
        {
            if (!dataService.SaveData($"/Maps/{mapName}.json", heightMap))
            {
                Debug.LogError("Could not save the data.");
                return;
            }
        }

        public HeightMap LoadMapData()
        {
            HeightMap loadedMap = dataService.LoadData<HeightMap>($"/Maps/{mapName}.json");
            return loadedMap ?? new HeightMap(mapSize);
        }
    }
}
