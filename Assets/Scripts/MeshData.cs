using System.Collections.Generic;
using UnityEngine;

namespace ProjectQuad
{
    public class MeshData
    {
        public List<Vector3> vertices;
        public List<int> triangles;
        public List<Vector2> uvs;

        private int vertexIndex;

        public MeshData(int chunkSize, int chunkHeight)
        {
            int capacity = chunkSize * chunkHeight * chunkSize;
            vertices = new List<Vector3>(capacity * 4);
            uvs = new List<Vector2>(capacity * 4);
            triangles = new List<int>(capacity * 6);
        }

        public void AddMesh(Vector3 origin, Tile tile)
        {
            foreach (Vector3 position in tile.mesh.positions)
            {
                vertices.Add(origin + position);
            }

            foreach (Vector2 uv in tile.atlasUVCoords)
            {
                uvs.Add(uv);
            }

            foreach (int index in tile.mesh.triangles)
            {
                triangles.Add(vertexIndex + index);
            }

            vertexIndex += tile.mesh.positions.Length;
        }

        public void ClearData()
        {
            vertices.Clear();
            triangles.Clear();
            uvs.Clear();
            vertexIndex = 0;
        }
    }
}