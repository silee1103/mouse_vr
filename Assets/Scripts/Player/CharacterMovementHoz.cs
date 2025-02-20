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
        // 목표 속도를 갱신
        if (!isAuto)
        {
            targetSpeed = PortConnect.instance.speed; // 아두이노 속도 데이터 가져오기
        }

        // 현재 속도를 목표 속도로 보간 (부드러운 속도 변화 적용)
        currentSpeed = Mathf.Lerp(currentSpeed, targetSpeed, 0.02f);
        
        // 움직임 적용
        if (Mathf.Abs(currentSpeed) > 0.1f) // 일정 속도 이상일 때만 이동
        {
            Vector3 moveDirection = transform.forward * (currentSpeed * speedWorldMul * Time.fixedDeltaTime);
            
            if (CheckHitWall(moveDirection)) // 벽과 충돌할 경우 이동하지 않음
                moveDirection = Vector3.zero;
                
            transform.Translate(moveDirection, Space.World);

            _anim.SetBool("running", true); // 이동 중 애니메이션 활성화
        }
        else
        {
            _anim.SetBool("running", false); // 정지 상태 애니메이션 활성화
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