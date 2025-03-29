using System.Collections;
using System.Linq;
using UnityEngine;

namespace ProjectQuad
{
    [ExecuteInEditMode]
    public class MapMeshGenerator : MonoBehaviour
    {
        readonly int chunkSize = Constants.CHUNK_SIZE;
        readonly int chunkHeight = Constants.CHUNK_HEIGHT;
        enum RuleTile
        {
            NARROW_N = 0,
            EDGE_NW = 1,
            EDGE_N = 2,
            EDGE_NE = 3,
            NARROW_VERTICAL = 4,
            EDGE_W = 5,
            GROUND = 6,
            EDGE_E = 7,
            NARROW_S = 8,
            EDGE_SW = 9,
            EDGE_S = 10,
            EDGE_SE = 11,
            BLOCK = 12,
            NARROW_W = 13,
            NARROW_HORIZONTAL = 14,
            NARROW_E = 15,
            WALL_W = 16,
            WALL_S = 17,
            WALL_E = 18,
            WALL_SW_BASE = 19,
            WALL_SW = 20,
            WALL_SW_TOP = 21,
            WALL_SE_BASE = 22,
            WALL_SE = 23,
            WALL_SE_TOP = 24,
            RULE_TILE_COUNT = 25
        };

        public Tileset tileset;

        Vector2Int mapSize;
        SerializableDictionary<Vector2Int, MapChunk> mapChunks;

        [SerializeField]
        GameObject mapChunkPrefab;

        MeshData meshData;

        readonly Vector2Int[] neighbourOffsets =
        {
            Vector2Int.up,
            Vector2Int.right,
            Vector2Int.down,
            Vector2Int.left
        };

        public void InitializeChunkDictionary()
        {
            mapChunks ??= new SerializableDictionary<Vector2Int, MapChunk>();

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

            int chunkSize = Constants.CHUNK_SIZE;

            int chunksX = (mapSize.x + (chunkSize - 1)) / chunkSize;
            int chunksZ = (mapSize.y + (chunkSize - 1)) / chunkSize;

            InitializeChunkDictionary();

            DeleteUnusedChunks(chunksX, chunksZ);

            for (int z = 0; z < chunksZ; z++)
            {
                for (int x = 0; x < chunksX; x++)
                {
                    Vector2Int chunkCoord = new(x, z);
                    if (!mapChunks.ContainsKey(chunkCoord))
                    {
                        InitializeChunk(chunkCoord);
                    }
                    GenerateChunk(chunkCoord, heightMap);
                }
            }
        }

        private void InitializeChunk(Vector2Int chunkCoord)
        {
            GameObject prefabInstance = Instantiate(mapChunkPrefab);
            prefabInstance.name = "Chunk " + chunkCoord.ToString();
            prefabInstance.transform.parent = transform;

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

        public void GenerateChunk(Vector2Int chunkCoord, HeightMap heightMap)
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
                    int tileHeight = heightMap.GetCell(tileCoord);
                    int[] neighbours = new int[neighbourOffsets.Length];

                    for (int i = 0; i < neighbours.Length; i++)
                    {
                        neighbours[i] = heightMap.GetCell(tileCoord + neighbourOffsets[i]);
                    }
                    PlaceAutoTiles(tileCoord, tileHeight, neighbours);
                }
            }
            chunk.RebuildMesh();
        }

        private BitArray GetBitmask(int tileHeight, int[] neighbours)
        {
            BitArray bitmask = new(neighbours.Length, false);
            for (int i = 0; i < neighbours.Length; i++)
            {
                if (tileHeight <= neighbours[i])
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

        private void PlaceAutoTiles(Vector2Int tileCoord, int tileHeight, int[] neighbours)
        {
            // Autotiling is determined by a bitmask, each neighbouring tile is assigned a bit (see below).

            // [ ] [1] [ ]
            // [8] [X] [2]
            // [ ] [4] [ ]

            // The bit is 0 if there is no tile in that direction and 1 if there is.
            // The bits are converted into an int between 0-15 which is used for the autotiling rules.

            BitArray bitArrayMask = GetBitmask(tileHeight, neighbours);
            int bitmask = BitArrayToInt(bitArrayMask);

            // Default ground tile
            Tile groundTile = GetTile(RuleTile.GROUND);

            switch (bitmask)
            {
                case 0:
                    groundTile = GetTile(RuleTile.BLOCK);
                    break;

                case 1:
                    groundTile = GetTile(RuleTile.NARROW_S);
                    break;

                case 2:
                    groundTile = GetTile(RuleTile.NARROW_W);
                    break;

                case 3:
                    groundTile = GetTile(RuleTile.EDGE_SW);

                    Vector3Int wallStartSW = new(tileCoord.x, Mathf.Max(neighbours[2], neighbours[3]), tileCoord.y);

                    if (neighbours[2] > neighbours[3])
                    {
                        PlaceGroundTile(wallStartSW, GetTile(RuleTile.EDGE_W));
                        PlaceWallTiles(tileCoord, neighbours[3], neighbours[2], GetTile(RuleTile.WALL_W));
                    }
                    else if (neighbours[3] > neighbours[2])
                    {
                        PlaceGroundTile(wallStartSW, GetTile(RuleTile.EDGE_S));
                        PlaceWallTiles(tileCoord, neighbours[2], neighbours[3], GetTile(RuleTile.WALL_S));
                    }
                    else
                    {
                        PlaceGroundTile(wallStartSW, GetTile(RuleTile.GROUND));
                    }
                    PlaceDiagWallTiles(tileCoord, wallStartSW.y, tileHeight, 
                        GetTile(RuleTile.WALL_SW_BASE), GetTile(RuleTile.WALL_SW), GetTile(RuleTile.WALL_SW_TOP));
                    break;

                case 4:
                    groundTile = GetTile(RuleTile.NARROW_N);
                    break;

                case 5:
                    groundTile = GetTile(RuleTile.NARROW_VERTICAL);
                    break;

                case 6:
                    groundTile = GetTile(RuleTile.EDGE_NW);

                    if (neighbours[3] < neighbours[0])
                    {
                        PlaceGroundTile(new Vector3(tileCoord.x, neighbours[0], tileCoord.y), GetTile(RuleTile.EDGE_W));
                        PlaceWallTiles(tileCoord, neighbours[3], neighbours[0], GetTile(RuleTile.WALL_W));
                    }
                    else
                    {
                        PlaceGroundTile(new Vector3(tileCoord.x, neighbours[0], tileCoord.y), GetTile(RuleTile.GROUND));
                    }
                    break;

                case 7:
                    groundTile = GetTile(RuleTile.EDGE_W);
                    break;

                case 8:
                    groundTile = GetTile(RuleTile.NARROW_E);
                    break;

                case 9:
                    groundTile = GetTile(RuleTile.EDGE_SE);

                    Vector3Int wallStartSE = new(tileCoord.x, Mathf.Max(neighbours[1], neighbours[2]), tileCoord.y);

                    if (neighbours[1] > neighbours[2])
                    {
                        PlaceGroundTile(wallStartSE, GetTile(RuleTile.EDGE_S));
                        PlaceWallTiles(tileCoord, neighbours[2], neighbours[1], GetTile(RuleTile.WALL_S));
                    }
                    else if (neighbours[2] > neighbours[1])
                    {
                        PlaceGroundTile(wallStartSE, GetTile(RuleTile.EDGE_E));
                        PlaceWallTiles(tileCoord, neighbours[1], neighbours[2], GetTile(RuleTile.WALL_E));
                    }
                    else
                    {
                        PlaceGroundTile(wallStartSE, GetTile(RuleTile.GROUND));
                    }
                    PlaceDiagWallTiles(tileCoord, wallStartSE.y, tileHeight,
                        GetTile(RuleTile.WALL_SE_BASE), GetTile(RuleTile.WALL_SE), GetTile(RuleTile.WALL_SE_TOP));
                    break;

                case 10:
                    groundTile = GetTile(RuleTile.NARROW_HORIZONTAL);
                    break;

                case 11:
                    groundTile = GetTile(RuleTile.EDGE_S);
                    break;

                case 12:
                    groundTile = GetTile(RuleTile.EDGE_NE);

                    if (neighbours[1] < neighbours[0])
                    {
                        PlaceGroundTile(new Vector3(tileCoord.x, neighbours[0], tileCoord.y), GetTile(RuleTile.EDGE_E));
                        PlaceWallTiles(tileCoord, neighbours[1], neighbours[0], GetTile(RuleTile.WALL_E));
                    }
                    else
                    {
                        PlaceGroundTile(new Vector3(tileCoord.x, neighbours[0], tileCoord.y), GetTile(RuleTile.GROUND));
                    }
                    break;

                case 13:
                    groundTile = GetTile(RuleTile.EDGE_E);
                    break;

                case 14:
                    groundTile = GetTile(RuleTile.EDGE_N);
                    break;

                case 15:
                    break;
            }

            PlaceGroundTile(new Vector3(tileCoord.x, tileHeight, tileCoord.y), groundTile);

            // Corner walls have already been dealt with, so they can be skipped
            if (bitmask is 3 or 6 or 9 or 12) return;

            if (!bitArrayMask[1])
            {
                PlaceWallTiles(tileCoord, neighbours[1], tileHeight, GetTile(RuleTile.WALL_E));
            }
            if (!bitArrayMask[2])
            {
                PlaceWallTiles(tileCoord, neighbours[2], tileHeight, GetTile(RuleTile.WALL_S));
            }
            if (!bitArrayMask[3])
            {
                PlaceWallTiles(tileCoord, neighbours[3], tileHeight, GetTile(RuleTile.WALL_W));
            }
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

        private void PlaceDiagWallTiles(Vector2Int tileCoord, int baseHeight, int topHeight, Tile baseTile, Tile wallTile, Tile topTile)
        {
            meshData.AddMesh(new Vector3(tileCoord.x, baseHeight, tileCoord.y), baseTile);
            for (int height = baseHeight + 1; height < topHeight; height++)
            {
                meshData.AddMesh(new Vector3(tileCoord.x, height, tileCoord.y), wallTile);
            }
            meshData.AddMesh(new Vector3(tileCoord.x, topHeight, tileCoord.y), topTile);
        }

        private Tile GetTile(RuleTile ruleTile)
        {
            return tileset.tiles[(int)ruleTile];
        }
    }
}