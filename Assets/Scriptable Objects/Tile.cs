using UnityEngine;

namespace ProjectQuad
{
    [System.Serializable]
    [CreateAssetMenu(menuName = "Scriptable Objects/Tile")]
    public class Tile : ScriptableObject
    {
        [Tooltip("The MeshDefinition that contains the data used to generate the tile mesh.")]
        public MeshDefinition mesh;
        [Tooltip("Rect defining the graphic. Top left is (0, 0).")]
        public RectInt textureRect = new(0, 0, Constants.TILE_SIZE, Constants.TILE_SIZE);

        [HideInInspector]
        public Vector2[] atlasUVCoords;
    }
}