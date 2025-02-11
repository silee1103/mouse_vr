using UnityEngine;

public class CameraMovement : MonoBehaviour
{
    public float defaultDuration = 1.0f; // 속도가 0일 때 애니메이션 주기
    public float activeDuration = 0.5f; // 속도가 0이 아닐 때 애니메이션 주기

    private float baseMinY = 0.54f; // 기본 최소 y 값
    private float baseMaxY = 0.545f; // 기본 최대 y 값
    private float runMinY = 0.535f; // 기본 최소 y 값
    private float runMaxY = 0.55f; // 기본 최대 y 값

    [SerializeField] private float currentMinY; // 현재 최소 y 값
    [SerializeField] private float currentMaxY; // 현재 최대 y 값
    [SerializeField] private float currentDuration; // 현재 애니메이션 주기

    private float targetMinY; // 목표 최소 y 값
    private float targetMaxY; // 목표 최대 y 값
    private float targetDuration; // 목표 애니메이션 주기

    public float lerpSpeed = 5f; // 보간 속도
    [SerializeField] private float smoothedSpeed = 0f; // 속도 변화 완화 변수
    private float speedLerpRate = 3f; // 속도 변화 보간 속도

    private void Start()
    {
        // 초기 값 설정
        currentMinY = baseMinY;
        currentMaxY = baseMaxY;
        currentDuration = defaultDuration;

        targetMinY = currentMinY;
        targetMaxY = currentMaxY;
        targetDuration = currentDuration;
    }

    private void Update()
    {
        // current 값을 target 값으로 부드럽게 보간
        currentMinY = Mathf.Lerp(currentMinY, targetMinY, Time.deltaTime * lerpSpeed);
        currentMaxY = Mathf.Lerp(currentMaxY, targetMaxY, Time.deltaTime * lerpSpeed);
        currentDuration = Mathf.Lerp(currentDuration, targetDuration, Time.deltaTime * lerpSpeed);

        // 현재 주기에 따라 애니메이션 진행
        float t = (Time.time % currentDuration) / currentDuration;

        // 애니메이션의 현재 y 좌표 계산
        float newY = Mathf.Lerp(currentMinY, currentMaxY, Mathf.PingPong(t * 2, 1));
        transform.position = new Vector3(transform.position.x, newY, transform.position.z);
    }

    public void UpdateAnimationMode(float speed)
    {
        // 속도 변화를 부드럽게 보간하여 적용
        smoothedSpeed = Mathf.Lerp(smoothedSpeed, speed, Time.deltaTime * speedLerpRate);

        if (speed < 0.01f)
        {
            // 속도가 0에 가까우면 기본 애니메이션
            targetMinY = baseMinY;
            targetMaxY = baseMaxY;
            targetDuration = defaultDuration;
        }
        else
        {
            // 속도가 있을 때 활성 애니메이션
            float adjustedSpeed = Mathf.SmoothStep(0, 1, Mathf.Abs(smoothedSpeed / 10f)); // 속도 변화 완화
            float minDuration = 0.3f; // 최소 지속 시간 설정
            float durationFactor = Mathf.Lerp(1, 0.5f, adjustedSpeed);

            targetMinY = runMinY;
            targetMaxY = runMaxY;
            targetDuration = Mathf.Max(defaultDuration * durationFactor, minDuration);
        }
    }
}
