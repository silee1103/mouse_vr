using UnityEngine;
using UnityEngine.UI;
using TMPro;

// UI에 할당 + 할당된 target의 rotation 에 따라 방향 및 위치 정보를 UI로 표시하는 클래스
// Prefab/UI/ColorWheel을 scene의 canvas에 두고 inspector에서 target에 객체 할당 
public class DirectionDisplay : MonoBehaviour
{
    [Header("UI Elements")]
    public Image directionCircle; // 방향 원 (기본적인 배경 역할, 색상 고정)
    public Image arrow; // 현재 방향을 나타내는 화살표 이미지
    public TMP_Text angleText; // 현재 각도(Y축 회전) 정보를 표시하는 텍스트
    public TMP_Text positionText; // 현재 위치(X, Z 좌표)를 표시하는 텍스트
    
    [Header("Target Object")]
    public Transform target; // 추적할 대상 오브젝트 (예: objectToReplay)

    void Update()
    {
        if (target is not null) // 대상이 존재할 경우에만 업데이트 수행
        {
            // 1. 대상의 Y축 회전 각도 가져오기
            float rotationY = target.rotation.eulerAngles.y;

            // 2. 화살표 이미지 회전 업데이트
            if (arrow is not null)
                arrow.rectTransform.localRotation = Quaternion.Euler(0, 0, rotationY); // Z축 반대로 회전
                // 화살표를 대상의 Y축 회전 값에 맞춰 Z축으로 회전 (UI 좌표계 기준)
            
            // 3. 회전 각도 텍스트 업데이트
            if (angleText is not null)
                angleText.text = $"Angle: {rotationY:F1}°";

            // 4. 위치 정보 텍스트 업데이트
            if (positionText is not null)
            {
                Vector3 pos = target.position;
                positionText.text = $"Position: ({pos.x:F2}, {pos.z:F2})";
            }
        }
    }
}
