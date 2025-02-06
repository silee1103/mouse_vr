using UnityEngine;

public class CameraMovement : MonoBehaviour
{
    public float defaultDuration = 0.5f; // 속도가 0일 때 애니메이션 주기
    public float activeDuration = 0.1f; // 속도가 0이 아닐 때 애니메이션 주기

    private float baseMinY = 0.54f; // 기본 최소 y 값
    private float baseMaxY = 0.545f; // 기본 최대 y 값
    // private float activeMinY = 0.27f; // 속도가 있을 때 최소 y 값
    // private float activeMaxY = 0.3f;  // 속도가 있을 때 최대 y 값
    private float underWidth = 0.03f;
    private float upperWidth = 0.03f;
    
    private float currentMinY; // 현재 최소 y 값
    private float currentMaxY; // 현재 최대 y 값
    private float currentDuration; // 현재 애니메이션 주기

    private void Start()
    {
        // 초기 값 설정
        currentMinY = baseMinY;
        currentMaxY = baseMaxY;
        currentDuration = defaultDuration;
    }

    private void Update()
    {
        // 현재 주기에 따라 애니메이션 진행
        float t = (Time.time % currentDuration) / currentDuration;

        // 애니메이션의 현재 y 좌표 계산
        float newY = Mathf.Lerp(currentMinY, currentMaxY, Mathf.PingPong(t * 2, 1));
        transform.position = new Vector3(transform.position.x, newY, transform.position.z);
    }

    public void UpdateAnimationMode(float speed)
    {
        if (speed <= 0.1f)
        {
            // 속도가 0에 가까우면 기본 애니메이션
            currentMinY = baseMinY;
            currentMaxY = baseMaxY;
            currentDuration = defaultDuration;
        }
        else
        {
            // 속도가 있을 때 활성 애니메이션
            currentMinY = baseMinY - underWidth * speed;
            currentMaxY = baseMinY + upperWidth * speed;
            currentDuration = activeDuration / (speed * 2);
        }
    }
}