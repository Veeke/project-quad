using UnityEngine;

namespace ProjectQuad
{
    [RequireComponent(typeof(MeshFilter), typeof(MeshRenderer), typeof(MeshCollider))]
    public class ChunkMesh : MonoBehaviour
    {
        Mesh mesh;
        MeshFilter meshFilter;
        MeshRenderer meshRenderer;
        MeshCollider meshCollider;
        MeshData meshData;
    }
}