using System.Collections;
using System.Linq;
using UnityEngine;

namespace ProjectQuad
{
    public class MarchingSquaresGenerator : MonoBehaviour, IMapMeshGenerator
    {
        readonly int chunkSize = Constants.CHUNK_SIZE;
        readonly int chunkHeight = Constants.CHUNK_HEIGHT;
        enum RuleTile
        {
            NULL = 0,
            OUTER_EDGE_NE = 1,
            OUTER_EDGE_NW = 2,
            EDGE_N = 3,
            EDGE_SW = 4,
            DIAGONAL_N = 5,
            EDGE_W = 6,
            INNER_EDGE_NW = 7,
            OUTER_EDGE_SE = 8,
            EDGE_E = 9,
            DIAGONAL_S = 10,
            INNER_EDGE_NE = 11,
            EDGE_S = 12,
            INNER_EDGE_SE = 13,
            INNER_EDGE_SW = 14,
            GROUND = 15,
            WALL_W = 16,
            WALL_E = 17,
            WALL_S = 18,
            OUTER_WALL_SW = 19,
            INNER_WALL_SW = 20,
            OUTER_WALL_SE = 21,
            INNER_WALL_SE = 22,
            RULE_TILE_COUNT = 23
        };

        public Tileset tileset;

        Vector2Int mapSize;
        SerializableDictionary<Vector2Int, MapChunk> mapChunks;

        [SerializeField]
        GameObject mapChunkPrefab;
        [SerializeField, HideInInspector]
        GameObject mapChunksContainer;

        MeshData meshData;

        readonly Vector2Int[] corners =
        {
            Vector2Int.zero,
            Vector2Int.right,
            Vector2Int.one,
            Vector2Int.up
        };

        public void Initialize()
        {
            mapChunks = new SerializableDictionary<Vector2Int, MapChunk>();

            if (mapChunksContainer != null)
            {
                DestroyImmediate(mapChunksContainer);
            }
            mapChunksContainer = new GameObject("Chunks");
            mapChunksContainer.transform.parent = transform;
        }

        public void DeleteUnusedChunks(int chunksX, int chunksZ)
        {
            var chunksToDelete = mapChunks.Where(c => c.Key.x >= chunksX || c.Key.y >= chunksZ).ToArray();
            foreach (var chunk in chunksToDelete)
            {
                DestroyImmediate(mapChunks[chunk.Key].gameObject);
                mapChunks.Remove(chunk.Key);
            }
        }

        public void GenerateMapMesh(HeightMap heightMap)
        {
            mapSize = heightMap.GetMapSize();

            if (mapChunks == null || mapChunks.Count == 0 || mapChunksContainer == null)
            {
                Initialize();
            }

            int chunksX = (mapSize.x + (chunkSize - 1)) / chunkSize;
            int chunksZ = (mapSize.y + (chunkSize - 1)) / chunkSize;

            DeleteUnusedChunks(chunksX, chunksZ);

            for (int z = 0; z < chunksZ; z++)
            {
                for (int x = 0; x < chunksX; x++)
                {
                    Vector2Int chunkCoord = new(x, z);

                    if (!mapChunks.ContainsKey(chunkCoord))
                    {
                        CreateChunk(chunkCoord);
                    }
                    else if (mapChunks[chunkCoord] == null)
                    {
                        mapChunks.Remove(chunkCoord);
                        CreateChunk(chunkCoord);
                    }
                    GenerateChunkMesh(chunkCoord, heightMap);
                }
            }
        }

        private void CreateChunk(Vector2Int chunkCoord)
        {
            GameObject prefabInstance = Instantiate(mapChunkPrefab);
            prefabInstance.name = "Chunk " + chunkCoord.ToString();
            prefabInstance.transform.parent = mapChunksContainer.transform;

            MapChunk mapChunk = prefabInstance.GetComponent<MapChunk>();
            Mesh chunkMesh = new()
            {
                name = "Chunk Mesh " + chunkCoord.ToString()
            };
            MeshData chunkMeshData = new(chunkSize, chunkHeight);
            mapChunk.InitializeMesh(chunkMesh, chunkMeshData, tileset.material);
            mapChunks.Add(chunkCoord, mapChunk);

            Debug.Log($"Chunk generated at coordinates {chunkCoord}");
        }

        public void GenerateChunkMesh(Vector2Int chunkCoord, HeightMap heightMap)
        {
            MapChunk chunk = mapChunks[chunkCoord];
            chunk.ClearMesh();
            meshData = chunk.GetMeshData();

            int chunkSizeX = Mathf.Clamp(mapSize.x - chunkSize * chunkCoord.x, 0, chunkSize);
            int chunkSizeZ = Mathf.Clamp(mapSize.y - chunkSize * chunkCoord.y, 0, chunkSize);

            Vector2Int origin = chunkCoord * chunkSize;

            for (int z = 0; z < chunkSizeZ; z++)
            {
                for (int x = 0; x < chunkSizeX; x++)
                {
                    Vector2Int tileCoord = origin + new Vector2Int(x, z);
                    int[] cornerHeights = new int[corners.Length];

                    for (int i = 0; i < cornerHeights.Length; i++)
                    {
                        cornerHeights[i] = heightMap.GetCell(tileCoord + corners[i]);
                    }
                    int tileHeight = cornerHeights.Max();
                    PlaceAutoTiles(tileCoord, tileHeight, cornerHeights);
                }
            }
            chunk.RebuildMesh();
        }

        private BitArray GetBitmask(int tileHeight, int[] cornerHeights)
        {
            BitArray bitmask = new(cornerHeights.Length, false);
            for (int i = 0; i < cornerHeights.Length; i++)
            {
                if (tileHeight <= cornerHeights[i])
                {
                    bitmask[i] = true;
                }
            }
            return bitmask;
        }

        private int BitArrayToInt(BitArray bitmask)
        {
            int[] result = new int[1];
            bitmask.CopyTo(result, 0);
            return result[0];
        }

        private void PlaceAutoTiles(Vector2Int tileCoord, int tileHeight, int[] cornerHeights)
        {
            // Autotiling is determined by the marching squares algorithm. The tiles are offset by half a tile relative to the grid.
            // The values are checked at the "corners" of the tile

            // (0,1)----(1,1)
            // |            |
            // |            |
            // |            |
            // |            |
            // (0,0)----(1,0)

            // The bit is 0 if the height is lower at that heightmap cell, and 1 if it's the same or higher.
            // The bits are converted into an int between 0-15 which is used for the autotiling rules.

            BitArray bitArrayMask = GetBitmask(tileHeight, cornerHeights);
            int bitmask = BitArrayToInt(bitArrayMask);

            switch (bitmask)
            {
                case 0:
                    break;

                case 1:
                    break;

                case 2:
                    break;

                case 3:
                    break;

                case 4:
                    PlaceDiagWallTiles(tileCoord, cornerHeights[0], cornerHeights[2], GetTile(RuleTile.OUTER_WALL_SW));
                    break;

                case 5:
                    PlaceDiagWallTiles(tileCoord, cornerHeights[1], cornerHeights[2], GetTile(RuleTile.INNER_WALL_SE));
                    break;

                case 6:
                    PlaceWallTiles(tileCoord, cornerHeights[0], cornerHeights[1], GetTile(RuleTile.WALL_W));
                    break;

                case 7:
                    break;

                case 8:
                    PlaceDiagWallTiles(tileCoord, cornerHeights[1], cornerHeights[3], GetTile(RuleTile.OUTER_WALL_SE));
                    break;

                case 9:
                    PlaceWallTiles(tileCoord, cornerHeights[1], cornerHeights[0], GetTile(RuleTile.WALL_E));
                    break;

                case 10:
                    PlaceDiagWallTiles(tileCoord, cornerHeights[0], cornerHeights[3], GetTile(RuleTile.INNER_WALL_SW));
                    break;

                case 11:
                    break;

                case 12:
                    PlaceWallTiles(tileCoord, cornerHeights[0], cornerHeights[2], GetTile(RuleTile.WALL_S));
                    break;

                case 13:
                    PlaceDiagWallTiles(tileCoord, cornerHeights[1], cornerHeights[3], GetTile(RuleTile.INNER_WALL_SE));
                    break;

                case 14:
                    PlaceDiagWallTiles(tileCoord, cornerHeights[0], cornerHeights[2], GetTile(RuleTile.INNER_WALL_SW));
                    break;

                case 15:
                    break;
            }

            PlaceGroundTile(new Vector3(tileCoord.x, tileHeight, tileCoord.y), tileset.tiles[bitmask]);
        }

        private void PlaceGroundTile(Vector3 tileCoord, Tile tile)
        {
            meshData.AddMesh(tileCoord, tile);
        }

        private void PlaceWallTiles(Vector2Int tileCoord, int baseHeight, int topHeight, Tile tile)
        {
            for (int height = baseHeight; height < topHeight; height++)
            {
                meshData.AddMesh(new Vector3(tileCoord.x, height, tileCoord.y), tile);
            }
        }

        private void PlaceDiagWallTiles(Vector2Int tileCoord, int baseHeight, int topHeight, Tile tile)
        {
            for (int height = baseHeight; height < topHeight; height++)
            {
                meshData.AddMesh(new Vector3(tileCoord.x, height, tileCoord.y), tile);
            }
        }

        private Tile GetTile(RuleTile ruleTile)
        {
            return tileset.tiles[(int)ruleTile];
        }

        public void RecalculateTilesetUV()
        {
            tileset.RecalculateUVs();
        }
    }
}