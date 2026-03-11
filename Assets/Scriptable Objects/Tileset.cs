using UnityEngine;

namespace ProjectQuad
{
    [CreateAssetMenu(menuName = "Scriptable Objects/Tileset")]
    public class Tileset : ScriptableObject
    {
        public Material material;
        public Tile[] tiles;

        private void OnValidate()
        {
            RecalculateUVs();
        }

        public void RecalculateUVs()
        {
            if (material == null)
            {
                Debug.LogError($"No material has been given for {name}.");
                return;
            }
            Texture textureAtlas = material.GetTexture("_BaseMap");
            Vector2 texelSize = textureAtlas.texelSize;

            foreach (var tile in tiles)
            {
                RectInt textureRect = tile.textureRect;
                Vector2[] uvs = tile.mesh.uvs;
                tile.atlasUVCoords = new Vector2[uvs.Length];

                for (int i = 0; i < uvs.Length; i++)
                {
                    Vector2 pixelCoords = new(
                        textureRect.xMin + uvs[i].x * textureRect.width,
                        textureRect.yMax - uvs[i].y * textureRect.height);
                    tile.atlasUVCoords[i] = new Vector2(pixelCoords.x * texelSize.x, 1 - pixelCoords.y * texelSize.y);
                }
            }
        }
    }
}