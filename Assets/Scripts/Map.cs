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

        [SerializeField, Range(-4, 11)]
        int brushHeight;
        [SerializeField, Range(1, 8)]
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
            if (GetCellHeight(x, y) == height) return;

            heightMap.SetCell(x, y, height);
            var chunksToRegen = GetEditedChunks(new Vector2Int(x, y));
            RegenChunks(chunksToRegen);
            SaveMapData();
        }

        public void PlaceTiles(List<Vector2Int> coords, int height)
        {
            List<Vector2Int> editedTiles = new(coords.Count);
            foreach (Vector2Int tileCoord in coords)
            {
                if (GetCellHeight(tileCoord.x, tileCoord.y) == height) continue;

                heightMap.SetCell(tileCoord.x, tileCoord.y, height);
                editedTiles.Add(tileCoord);
            }
            var chunksToRegen = GetEditedChunks(editedTiles);
            RegenChunks(chunksToRegen);
            SaveMapData();
        }

        public HashSet<Vector2Int> GetEditedChunks(Vector2Int tileCoord)
        {
            HashSet<Vector2Int> editedChunks = new();
            Vector2Int chunkCoord = tileCoord / chunkSize;

            editedChunks.Add(chunkCoord);
            if (tileCoord.x % chunkSize == 0 && tileCoord.x != 0)
                editedChunks.Add(chunkCoord + Vector2Int.left);
            if ((tileCoord.x + 1) % chunkSize == 0 && tileCoord.x != mapSize.x)
                editedChunks.Add(chunkCoord + Vector2Int.right);
            if (tileCoord.y % chunkSize == 0 && tileCoord.y != 0)
                editedChunks.Add(chunkCoord + Vector2Int.down);
            if ((tileCoord.y + 1) % chunkSize == 0 && tileCoord.y != mapSize.y)
                editedChunks.Add(chunkCoord + Vector2Int.up);

            return editedChunks;
        }

        public HashSet<Vector2Int> GetEditedChunks(List<Vector2Int> tileCoords)
        {
            HashSet<Vector2Int> editedChunks = new();
            
            foreach (Vector2Int tileCoord in tileCoords)
            {
                Vector2Int chunkCoord = tileCoord / chunkSize;

                editedChunks.Add(chunkCoord);
                if (tileCoord.x % chunkSize == 0 && tileCoord.x != 0)
                    editedChunks.Add(chunkCoord + Vector2Int.left);
                if ((tileCoord.x + 1) % chunkSize == 0 && tileCoord.x != mapSize.x)
                    editedChunks.Add(chunkCoord + Vector2Int.right);
                if (tileCoord.y % chunkSize == 0 && tileCoord.y != 0)
                    editedChunks.Add(chunkCoord + Vector2Int.down);
                if ((tileCoord.y + 1) % chunkSize == 0 && tileCoord.y != mapSize.y)
                    editedChunks.Add(chunkCoord + Vector2Int.up);
            }
            return editedChunks;
        }

        public void RegenChunks(HashSet<Vector2Int> editedChunks)
        {
            foreach (Vector2Int chunkCoord in editedChunks)
            {
                if (chunkCoord.x < 0 || chunkCoord.y < 0) continue;
                if (chunkCoord.x > (mapSize.x - 1) / chunkSize || chunkCoord.y > (mapSize.y - 1) / chunkSize) continue;

                Debug.Log($"Chunk {chunkCoord} has been regenerated");
                meshGenerator.GenerateChunk(chunkCoord, heightMap);
            }
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
