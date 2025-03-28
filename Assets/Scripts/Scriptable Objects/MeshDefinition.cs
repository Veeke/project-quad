using UnityEngine;

namespace ProjectQuad
{
    [CreateAssetMenu(menuName = "Scriptable Objects/Mesh Definition")]
    public class MeshDefinition : ScriptableObject
    {
        public Vector3[] positions;
        public int[] triangles;
        public Vector2[] uvs;
    }
}