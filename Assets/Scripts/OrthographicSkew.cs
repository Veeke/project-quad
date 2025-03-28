using UnityEngine;

[ExecuteInEditMode]
public class OrthographicSkew : MonoBehaviour
{
    [SerializeField]
    private Camera cam;
    [SerializeField, Range(0, 1)]
    private float shearFactor;

    private Matrix4x4 projectionMatrix;

    [ContextMenu("Print Projection Matrix")]
    private void PrintProjectionMatrix()
    {
        Debug.Log(projectionMatrix);
    }

    [ContextMenu("Reset Projection Matrix")]
    private void ResetProjectionMatrix()
    {
        cam.ResetProjectionMatrix();
        projectionMatrix = cam.projectionMatrix;
    }

    private void Update()
    {
        if (cam.orthographic)
        {
            UpdateCameraMatrix();
        }
    }

    public void UpdateCameraMatrix()
    {
        ResetProjectionMatrix();

        // Shear along z-axis based on y-axis
        projectionMatrix.m12 = projectionMatrix.m11 * shearFactor;

        // Translate the camera to make the shear relative to world space instead of camera space
        // The formula for the translating the view vertically is -((t + b) / (t - b)).
        // If we know that b = -t, when we add an offset the value we need is
        // -((t + z + b + z) / (t + z - (b + z)) =
        // -((t - t + z + z) / (t + t + z - z) =
        // -2z / 2t =
        // -z / t
        // Where z is the desired offset in units and t is the orthographic size of the camera
        projectionMatrix.m13 = transform.position.y / cam.orthographicSize * shearFactor;

        cam.projectionMatrix = projectionMatrix;
    }
}
