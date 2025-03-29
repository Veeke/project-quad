using UnityEngine;
using System.Collections.Generic;
using UnityEditor.ShaderGraph.Internal;

namespace ProjectQuad
{
    [ExecuteInEditMode]
    [RequireComponent (typeof(MapMeshGenerator))]
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
        [SerializeField, Range(1, 12)]
        int brushSize = 1;

        HeightMap heightMap;
        readonly int chunkSize = Constants.CHUNK_SIZE;

        MapMeshGenerator meshGenerator;
        JsonDataService dataService = new();

        public void InitializeMap()
        {
            heightMap ??= LoadMapData();
            meshGenerator = GetComponent<MapMeshGenerator>();
            meshGenerator.InitializeChunkDictionary();
        }

        public void PlaceTile(int x, int y, int height)
        {
            heightMap.SetCell(x, y, height);
            RegenChunks(x, y);
            SaveMapData();
        }

        public HashSet<Vector2Int> GetEditedChunks(Vector2Int[] coords)
        {
            HashSet<Vector2Int> editedChunks = new();
            
            for (int i = 0; i < coords.Length; i++)
            {
                Vector2Int tileCoords = new(coords[i].x, coords[i].y);
                Vector2Int chunkCoord = tileCoords / chunkSize;
                if (chunkCoord.x < 0 || chunkCoord.y < 0) 
                    continue;
                editedChunks.Add(chunkCoord);
            }
            return editedChunks;
        }

        public void RegenChunks(int x, int y)
        {
            Vector2Int chunkCoord = new(x / chunkSize, y / chunkSize);

            meshGenerator.GenerateChunk(chunkCoord, heightMap);
            if (x % chunkSize == 0 && x != 0)
            {
                meshGenerator.GenerateChunk(chunkCoord + Vector2Int.left, heightMap);
            }
            if ((x + 1) % chunkSize == 0 && x != mapSize.x)
            {
                meshGenerator.GenerateChunk(chunkCoord + Vector2Int.right, heightMap);
            }
            if (y % chunkSize == 0 && y != 0)
            {
                meshGenerator.GenerateChunk(chunkCoord + Vector2Int.down, heightMap);
            }
            if ((y + 1) % chunkSize == 0 && y != mapSize.y)
            {
                meshGenerator.GenerateChunk(chunkCoord + Vector2Int.up, heightMap);
            }
        }

        public void PlaceTiles(Vector2Int[] coords, int height)
        {
        }

        [ContextMenu("Reload Map")]
        public void ReloadMap()
        {
            heightMap = LoadMapData();
            if (heightMap.GetMapSize() != mapSize)
            {
                heightMap.SetMapSize(mapSize);
                SaveMapData();
            }
            tileset.RecalculateUVs();
            meshGenerator.GenerateMapMesh(heightMap);
        }

        public int GetCellHeight(int x, int y)
        {
            return heightMap.GetCell(x, y);
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
            if (loadedMap == null)
            {
                loadedMap = new HeightMap(mapSize);
                SaveMapData();
            }
            return loadedMap;
        }
    }
}
