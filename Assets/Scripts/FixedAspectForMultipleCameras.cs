using UnityEngine;

[RequireComponent(typeof(Camera))]
public class FixedAspectForMultipleCameras : MonoBehaviour
{
    public float targetAspect = 1.0f; // 예를 들어 1:1 (지금 화면처럼 완전 정사각형)
    private Camera cam;

    private void Start()
    {
        cam = GetComponent<Camera>();
        UpdateViewport();
    }

    void UpdateViewport()
    {
        float windowAspect = (float)Screen.width / (float)Screen.height;
        float scaleHeight = windowAspect / targetAspect;

        if (scaleHeight < 1.0f)
        {
            // 세로가 더 길다 → 위아래에 검은 여백
            Rect rect = new Rect(0, (1.0f - scaleHeight) / 2.0f, 1, scaleHeight);
            cam.rect = rect;
        }
        else
        {
            // 가로가 더 길다 → 좌우에 검은 여백
            float scaleWidth = 1.0f / scaleHeight;
            Rect rect = new Rect((1.0f - scaleWidth) / 2.0f, 0, scaleWidth, 1);
            cam.rect = rect;
        }
    }
}