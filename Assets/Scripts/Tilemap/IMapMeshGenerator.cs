using UnityEngine;

namespace ProjectQuad
{
    public interface IMapMeshGenerator
    {
        void RecalculateTilesetUV();
        void GenerateMapMesh(HeightMap heightMap);
        void GenerateChunkMesh(Vector2Int chunkCoord, HeightMap heightMap);
    }
}