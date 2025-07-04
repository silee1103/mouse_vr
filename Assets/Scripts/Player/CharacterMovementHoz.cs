using UnityEngine;

// 캐릭터의 수평 이동을 제어하는 클래스
// - 목표 속도를 설정하고 보간하여 부드럽게 이동
// - 벽 충돌 감지 기능 포함
// - 애니메이션 상태 업데이트
public class CharacterMovementHoz : MonoBehaviour
{
    [Header("Speed Settings")]
    private float speedWorldMul = 1.08f / 12f; // 속도 보정 (게임 내 크기 / 실제 크기 비율)
    
    [Header("Animation & Movement")]
    private Animator _anim; // 캐릭터 애니메이션 컨트롤러
    private float lastUpdateTime = 0f; // 속도 갱신 주기 관리
    private float currentSpeed = 0f; // 현재 이동 속도
    public float targetSpeed = 0f; // 목표 이동 속도

    public bool isAuto = false; // 자동 이동 모드 여부

    private float _colliderYSize; // 캐릭터 충돌체 높이

    void Start()
    {
        // 애니메이션 및 충돌 크기 초기화
        _anim = GetComponentInChildren<Animator>();
        _colliderYSize = GetComponent<BoxCollider>().size.y;
    }

    private void FixedUpdate()
    {
        if (!isAuto)
        {
            targetSpeed = PortConnect.instance.speed;
        }

        // Δy 크기에 따라 보간 계수 변화
        float deltaY = Mathf.Abs(PortConnect.instance.lastDeltaY);

        // 필터 감쇠 강도 (Δy가 작을수록 감쇠 빠르게)
        float baseLerp = 0.02f;          // 큰 Δy일 때 기본 감쇠율
        float maxLerp = 0.35f;           // 아주 작은 Δy일 때 최대 감쇠율
        float lerpFactor = Mathf.Lerp(maxLerp, baseLerp, Mathf.InverseLerp(0f, 1000f, deltaY));

        // 속도 보간 적용
        currentSpeed = Mathf.Lerp(currentSpeed, targetSpeed, lerpFactor);

        // 이동 및 애니메이션 처리
        if (Mathf.Abs(currentSpeed) > 0.1f)
        {
            Vector3 moveDirection = - transform.forward * (currentSpeed * speedWorldMul * Time.fixedDeltaTime);
            if (CheckHitWall(moveDirection))
                moveDirection = Vector3.zero;

            transform.Translate(moveDirection, Space.World);
            _anim.SetBool("running", true);
        }
        else
        {
            _anim.SetBool("running", false);
        }
    }


    // 벽 충돌 감지 함수
    bool CheckHitWall(Vector3 movement)
    {
        movement = transform.TransformDirection(movement);
        float detectionRange = 1f; // 충돌 감지 거리

        // 충돌 감지 Raycast (캐릭터의 중앙에서 위쪽으로 살짝 올려서 검사)
        Vector3 rayOrigin = transform.position + Vector3.up * (_colliderYSize * 0.5f);
        
        if (Physics.Raycast(rayOrigin, movement, out RaycastHit hit, detectionRange))
        {
            if (hit.collider.CompareTag("Wall")) // 벽과 충돌하면 true 반환
                return true;
        }
        
        return false;
    }
}



/*private void FixedUpdate()
    {
        if (!isAuto)
        {
            // 목표 속도를 주기적으로 갱신
            if (Time.time - lastUpdateTime >= 0.1)
            {
                targetSpeed = Input.GetAxis("Vertical") * 50;
                lastUpdateTime = Time.time; // 마지막 갱신 시간 업데이트
            }
        }
        // 현재 속도를 목표 속도로 보간
        currentSpeed = Mathf.Lerp(currentSpeed, targetSpeed, 0.1f);

        // 움직임 적용
        if (Mathf.Abs(currentSpeed) > 0.1f)
        {
            Vector3 moveDirection = transform.forward * (currentSpeed * speedWorldMul * Time.fixedDeltaTime);
            if (CheckHitWall(moveDirection))
                moveDirection = Vector3.zero;
            transform.Translate(moveDirection, Space.World);

            _anim.SetBool("running", true);
        }
        else
        {
            _anim.SetBool("running", false);
        }

        // 카메라 진동 폭 업데이트
        float normalizedSpeed = Mathf.Clamp01(Mathf.Abs(currentSpeed) / 10f); // 0~10의 값을 0~1로 변환

    }*/