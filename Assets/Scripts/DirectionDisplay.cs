using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DirectionDisplay : MonoBehaviour
{
    public Image directionCircle; // 방향 원 (고정된 색상)
    public Image arrow; // 화살표
    public TMP_Text angleText; // 각도 표시 텍스트
    public TMP_Text positionText; // 위치 표시 텍스트
    public Transform target; // 대상 Transform (예: objectToReplay)

    void Update()
    {
        if (target is not null)
        {
            // Y축 회전 각도 계산
            float rotationY = target.rotation.eulerAngles.y;

            // 화살표 회전 업데이트
            if (arrow is not null)
                arrow.rectTransform.localRotation = Quaternion.Euler(0, 0, rotationY); // Z축 반대로 회전

            // 텍스트 업데이트
            if (angleText is not null)
                angleText.text = $"Angle: {rotationY:F1}°";

            if (positionText is not null)
            {
                Vector3 pos = target.position;
                positionText.text = $"Position: ({pos.x:F2}, {pos.z:F2})";
            }
        }
    }
}
