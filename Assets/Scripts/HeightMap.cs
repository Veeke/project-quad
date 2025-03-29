using Newtonsoft.Json;
using UnityEngine;

namespace ProjectQuad
{
    public class HeightMap
    {
        [JsonProperty] Vector2Int mapSize;
        [JsonProperty] int[,] heightMap;

        public HeightMap(Vector2Int mapSize)
        {
            this.mapSize = mapSize;
            heightMap = new int[mapSize.x, mapSize.y];
        }

        public int GetCell(int x, int y)
        {
            if (x >= 0 && y >= 0 && x < mapSize.x && y < mapSize.y)
            {
                return heightMap[x, y];
            }
            return 0;
        }
        public int GetCell(Vector2Int coords)
        {
            if (coords.y < 0)
            {
                return 0;
            }
            coords.x = Mathf.Clamp(coords.x, 0, mapSize.x - 1);
            coords.y = Mathf.Clamp(coords.y, 0, mapSize.y - 1);
            return heightMap[coords.x, coords.y];
        }

        public void SetCell(int x, int y, int height)
        {
            if (IsCellWithinBounds(x, y))
            {
                heightMap[x, y] = height;
            }
        }

        public bool IsCellWithinBounds(int x, int y)
        {
            return x >= 0 && y >= 0 && x < mapSize.x && y < mapSize.y;
        }

        public Vector2Int GetMapSize()
        {
            return mapSize;
        }

        public void SetMapSize(Vector2Int newMapSize)
        {
            int[,] newHeightmap = new int[newMapSize.x, newMapSize.y];

            int minX = Mathf.Min(mapSize.x, newMapSize.x);
            int minY = Mathf.Min(mapSize.y, newMapSize.y);

            for (int y = 0; y < minY; y++)
            {
                for (int x = 0; x < minX; x++)
                {
                    newHeightmap[x, y] = heightMap[x, y];
                }
            }
            heightMap = newHeightmap;
            mapSize = newMapSize;
        }
    }
}