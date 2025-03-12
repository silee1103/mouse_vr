using UnityEngine;
using UnityEngine.UI;

public class TwoPassRender : MonoBehaviour
{
    public Texture inputTexture;    // 어떤 소스 텍스처 (ex: Texture2D, RenderTexture 등)
    public Material CylMaterial;    
    public Material PolarMaterial;
    public RawImage rawImage;

    [SerializeField] private RenderTexture cylRT;
    [SerializeField] private RenderTexture finalRT;

    void Start()
    {
        // 1) 중간용 RT, 최종용 RT 생성
        int width = 3840, height = 1080;
        cylRT   = new RenderTexture(width, height, 0, RenderTextureFormat.ARGB32);
        finalRT = new RenderTexture(width, height, 0, RenderTextureFormat.ARGB32);

        // 2) 첫 번째 Blit: inputTexture → cylRT (CylMaterial)
        Graphics.Blit(inputTexture, finalRT, PolarMaterial);

        // 3) 두 번째 Blit: cylRT → finalRT (PolarMaterial)
        // Graphics.Blit(cylRT, finalRT, PolarMaterial);

        // 이제 finalRT에 "Cyl → Polar"가 적용된 최종 결과가 들어 있음
        // 필요하면 UI나 Mesh에 finalRT를 표시 가능
        // 3) UI.RawImage에 finalRT 할당
        if (rawImage != null)
        {
            rawImage.texture = finalRT;
        }
    }

    void OnDestroy()
    {
        if (cylRT   != null) cylRT.Release();
        if (finalRT != null) finalRT.Release();
    }
}