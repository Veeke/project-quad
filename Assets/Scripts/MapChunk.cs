using UnityEngine;

namespace ProjectQuad
{
    [ExecuteInEditMode]
    [RequireComponent(typeof(MeshFilter), typeof(MeshRenderer), typeof(MeshCollider))]
    public class MapChunk : MonoBehaviour
    {
        Mesh mesh;
        MeshFilter meshFilter;
        MeshRenderer meshRenderer;
        MeshCollider meshCollider;
        MeshData meshData;

        private void Awake()
        {
            meshFilter = GetComponent<MeshFilter>();
            meshRenderer = GetComponent<MeshRenderer>();
            meshCollider = GetComponent<MeshCollider>();
        }

        public MeshData GetMeshData()
        {
            return meshData;
        }

        public void InitializeMesh(Mesh chunkMesh, MeshData chunkMeshData, Material material)
        {
            mesh = chunkMesh;
            meshFilter.mesh = mesh;
            meshCollider.sharedMesh = mesh;
            meshRenderer.sharedMaterial = material;
            meshData = chunkMeshData;
        }

        public void RebuildMesh()
        {
            mesh.SetVertices(meshData.vertices);
            mesh.SetTriangles(meshData.triangles, 0);
            mesh.SetUVs(0, meshData.uvs);
            mesh.RecalculateNormals();

            // Enabling and disabling a Mesh Collider updates it, for some reason.
            meshCollider.enabled = false;
            meshCollider.enabled = true;
        }

        public void ClearMesh()
        {
            if (mesh != null && meshData != null)
            {
                mesh.Clear();
                meshData.ClearData();
            }
            else
            {
                Debug.LogError($"Chunk {gameObject.name}'s mesh is not initialized properly.");
            }
        }
    }
}