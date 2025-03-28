using UnityEngine;

[ExecuteInEditMode]
public class PerspectiveSkew : MonoBehaviour
{
    [SerializeField]
    private Camera cam;

    [SerializeField]
    private Transform player;

    [SerializeField, Range(0, 1)]
    private float shearFactor;

    private Matrix4x4 viewMatrix;
    private Matrix4x4 shearMatrix = Matrix4x4.identity;

    [ContextMenu("Print View Matrix")]
    private void PrintViewMatrix()
    {
        Debug.Log(viewMatrix);
    }

    [ContextMenu("Reset View Matrix")]
    private void ResetViewMatrix()
    {
        cam.ResetWorldToCameraMatrix();
        viewMatrix = cam.worldToCameraMatrix;
    }

    private void Update()
    {
        transform.position = player.position;
        
        UpdateCameraMatrix();
    }

    public void UpdateCameraMatrix()
    {
        ResetViewMatrix();
        shearMatrix.m21 = shearFactor * Mathf.Cos(transform.rotation.eulerAngles.y * Mathf.Deg2Rad) * Mathf.Sin(transform.rotation.eulerAngles.x * 2 * Mathf.Deg2Rad);
        if (shearFactor > 0)
        {
            viewMatrix *= shearMatrix;
        }
        cam.worldToCameraMatrix = viewMatrix;
    }
}
