using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterMovement : MonoBehaviour
{
    public float speedWorldMul = 1/10f; // rat이 지금 실제보다 6배 작음
    public float rotationSpeed = 360f; // 회전 속도 (초당 회전 각도)
    
    private Animator _anim;
    
    [SerializeField]
    private PortConnect pm;
    
    [SerializeField]
    private CameraMovement cameraMovement; // CameraMovement 연결
    
    // 주기를 제어하기 위한 변수
    private float lastUpdateTime = 0f;
    private float currentSpeed = 0f; // 현재 속도
    private float targetSpeed = 0f; // 목표 속도
    
    void Start()
    {
        _anim = GetComponentInChildren<Animator>();
        pm = PortConnect.pm;
    }

    /*void FixedUpdate()
    {
        // 입력 받기
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");

        // 회전 처리 (좌/우)
        if (Mathf.Abs(horizontal) > 0.1f)
        {
            float rotationAmount = horizontal * rotationSpeed * Time.fixedDeltaTime;
            transform.Rotate(0f, rotationAmount, 0f); // Y축 기준 회전
        }

        // 이동 처리 (전/후)
        if (Mathf.Abs(vertical) > 0.1f)
        {
            Vector3 forward = transform.forward; // 현재 바라보는 방향
            Vector3 moveDirection = forward * (vertical * moveSpeed * Time.fixedDeltaTime);
            transform.Translate(moveDirection, Space.World); // 월드 좌표계 기준 이동

            _anim.SetBool("running", true);
        }
        else
        {
            _anim.SetBool("running", false);
        }
    }*/
    
    private void FixedUpdate()
    {
        // 목표 속도를 주기적으로 갱신
        if (Time.time - lastUpdateTime >= pm.sendInterval)
        {
            targetSpeed = pm.speed * 100;
            lastUpdateTime = Time.time; // 마지막 갱신 시간 업데이트
        }

        // 현재 속도를 목표 속도로 보간
        currentSpeed = Mathf.Lerp(currentSpeed, targetSpeed, 0.1f);
        
        // 움직임 적용
        if (Mathf.Abs(currentSpeed) > 0.1f)
        {
            Vector3 moveDirection = transform.forward * (currentSpeed * speedWorldMul * Time.fixedDeltaTime);
            transform.Translate(moveDirection, Space.World);

            _anim.SetBool("running", true);
        }
        else
        {
            _anim.SetBool("running", false);
        }
        
        // 카메라 진동 폭 업데이트
        float normalizedSpeed = Mathf.Clamp01(currentSpeed / 10f); // 0~10의 값을 0~1로 변환
        cameraMovement.UpdateAnimationMode(normalizedSpeed);
        
    }
}

// rigidbody velocity ver.
/*
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterMovement : MonoBehaviour
{
    public float moveSpeed = 5f; // 이동 속도
    public float rotationSpeed = 720f; // 회전 속도 (초당 회전 각도)
    
    private Rigidbody _rb;
    private Animator _anim;
    
    public float minYRotation = -5f; // 최소 Y축 회전 값
    public float maxYRotation = 5f; // 최대 Y축 회전 값
    public float returnToDefaultSpeed = 2f; // 초기 각도로 돌아가는 속도
    public float returnDelay = 1f; // 복구 대기 시간
    
    private Quaternion defaultRotation; // 초기 회전 값
    private float returnTimer; // 복구 대기 타이머
    
    
    [SerializeField]
    private PortConnect pm;
    
    // 주기를 제어하기 위한 변수
    private float lastUpdateTime = 0f;
    
    void Start()
    {
        _rb = GetComponent<Rigidbody>();
        _anim = GetComponentInChildren<Animator>();
        
        defaultRotation = transform.rotation;
    }

    void FixedUpdate()
    {
        // 입력 받기
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");

        // 움직임 벡터 계산 (회전은 절대값, 속도는 원래 값을 유지)
        Vector3 moveDirection = new Vector3(horizontal, 0f, Mathf.Abs(vertical));
        Vector3 moveVelocity = new Vector3(horizontal, 0f, vertical).normalized * moveSpeed;
        
        // 움직임 적용
        if (moveDirection.magnitude > 0.1f)
        {
            // 캐릭터를 이동
            _rb.velocity = new Vector3(moveVelocity.x, _rb.velocity.y, moveVelocity.z);

            // 캐릭터 회전
            Quaternion targetRotation = Quaternion.LookRotation(moveDirection);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);

            // Y축 회전 제한
            Vector3 euler = transform.eulerAngles;
            euler.y = ClampYRotation(euler.y);
            transform.eulerAngles = euler;

            // 회전 중에는 복구 대기 타이머 초기화
            returnTimer = 0f;

            _anim.SetBool("running", true);
        }
        else
        {
            // 입력이 없을 경우 속도 0
            _rb.velocity = new Vector3(0f, _rb.velocity.y, 0f);
            _anim.SetBool("running", false);

            // 입력이 없을 경우 복구 대기 타이머 증가
            returnTimer += Time.deltaTime;

            // 대기 시간이 지난 경우 초기 회전으로 복구
            if (returnTimer >= returnDelay)
            {
                transform.rotation = Quaternion.Lerp(transform.rotation, defaultRotation, returnToDefaultSpeed * Time.deltaTime);
            }
        }
    }
    
    /*void FixedUpdate()
    {
        // 현재 시간과 마지막 갱신 시간을 비교
        if (Time.time - lastUpdateTime >= pm.sendInterval)
        {
            UpdateCharacterMovement(); // 움직임 업데이트
            lastUpdateTime = Time.time; // 마지막 갱신 시간 업데이트
        }
        else
        {
            // 대기 시간이 지나면 초기 각도로 복구
            returnTimer += Time.deltaTime;
            if (returnTimer >= returnDelay)
            {
                transform.rotation = Quaternion.Lerp(transform.rotation, defaultRotation, returnToDefaultSpeed * Time.deltaTime);
            }
        }
    }#1#

    private void UpdateCharacterMovement()
    {
        // PortConnect에서 speed 가져오기
        Vector3 moveDirection = new Vector3(0f, 0f, pm.speed);

        // 움직임 적용
        if (moveDirection.magnitude > 0.1f)
        {
            // 캐릭터를 이동
            Vector3 moveVelocity = moveDirection.normalized * moveSpeed;
            _rb.velocity = new Vector3(moveVelocity.x, _rb.velocity.y, moveVelocity.z);

            // 캐릭터 회전 (Z축 이동 방향으로 회전)
            Quaternion targetRotation = Quaternion.LookRotation(moveDirection);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);

            // Y축 회전 제한
            Vector3 euler = transform.eulerAngles;
            euler.y = ClampYRotation(euler.y);
            transform.eulerAngles = euler;

            // 회전 중에는 복구 대기 타이머 초기화
            returnTimer = 0f;
            
            // 애니메이션 실행
            _anim.SetBool("running", true);
        }
        else
        {
            // 입력이 없을 경우 속도 0
            _rb.velocity = new Vector3(0f, _rb.velocity.y, 0f);
            _anim.SetBool("running", false);
        }
    }
    
    // Y축 회전을 제한하는 함수
    private float ClampYRotation(float currentY)
    {
        // -180 ~ 180 범위로 변환
        if (currentY > 180f) currentY -= 360f;
        // 제한된 범위 내로 클램프
        return Mathf.Clamp(currentY, minYRotation, maxYRotation);
    }
}
*/
