using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UIElements;

namespace ProjectQuad
{
    [System.Serializable]
    public class MeshData
    {
        public List<Vector3> vertices;
        public List<int> triangles;
        public List<Vector2> uvs;
        public List<Color> colors;

        private int vertexIndex;

        public MeshData(int chunkSize, int chunkHeight)
        {
            int capacity = chunkSize * chunkHeight * chunkSize;
            vertices = new List<Vector3>(capacity * 4);
            uvs = new List<Vector2>(capacity * 4);
            triangles = new List<int>(capacity * 6);
            colors = new List<Color>(capacity * 4);
        }

        public void AddMesh(Vector3 origin, Tile tile)
        {
            bool hasVertexColors = tile.mesh.colors?.Length > 0;

            for (int i = 0; i < tile.mesh.positions.Length; i++) 
            {
                vertices.Add(origin + tile.mesh.positions[i]);
                Vector2 atlasUVCoords = tile.atlasUVCoords[i];
                uvs.Add(atlasUVCoords);

                float a = hasVertexColors ? tile.mesh.colors[i].a : 1;
                Color vertexColor = new(atlasUVCoords.x, atlasUVCoords.y, 0, a);
                colors.Add(vertexColor);
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
            colors.Clear();
            vertexIndex = 0;
        }
    }
}