using UnityEngine;

[ExecuteInEditMode]
public class FisheyePostProcess : MonoBehaviour
{
    public Material fisheyeMaterial;

    void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        if (fisheyeMaterial != null)
        {
            // Apply the fisheye shader material
            Graphics.Blit(source, destination, fisheyeMaterial);
        }
        else
        {
            // If no material, just pass through
            Graphics.Blit(source, destination);
        }
    }
}