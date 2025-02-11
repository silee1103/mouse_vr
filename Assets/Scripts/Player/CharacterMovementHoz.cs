using UnityEngine;
using System.Collections.Generic;

public class CharacterMovementHoz : MonoBehaviour
{
    public float speedWorldMul = 1.08f/12f; // by z axis, in game rat size / rat real size (cm)
    public float rotationSpeed = 360f; // 회전 속도 (초당 회전 각도)
    
    private Animator _anim;
    
    [SerializeField]
    private CameraMovement cameraMovement; // CameraMovement 연결
    
    // 주기를 제어하기 위한 변수
    private float lastUpdateTime = 0f;
    private float currentSpeed = 0f; // 현재 속도
    public float targetSpeed = 0f; // 목표 속도

    public bool isAuto = false;

    private float _colliderYSize;
    
    void Start()
    {
        _anim = GetComponentInChildren<Animator>();
        _colliderYSize = GetComponent<BoxCollider>().size.y;
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
        cameraMovement.UpdateAnimationMode(normalizedSpeed);
        
    }*/
    
    private void FixedUpdate()
    {
        // 목표 속도를 주기적으로 갱신
        // 🔹 0.1초마다 targetSpeed 업데이트
        if (!isAuto && Time.time - lastUpdateTime >= 0.05f)
        {
            targetSpeed = PortConnect.instance.speed;
            // cameraMovement.UpdateAnimationMode(PortConnect.instance.speed);
            lastUpdateTime = Time.time; // 마지막 갱신 시간 업데이트
        }


        // 현재 속도를 목표 속도로 보간
        currentSpeed = Mathf.Lerp(currentSpeed, targetSpeed, 0.5f);
        
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

    }
    
    bool CheckHitWall(Vector3 movement)
    {
        movement = transform.TransformDirection(movement);
        float scope = 1f;

        Vector3 ray = transform.position + Vector3.up * (_colliderYSize * 0.5f);
        
        if (Physics.Raycast(ray, movement, out RaycastHit hit, scope))
        {
            if (hit.collider.CompareTag("Wall"))
                return true;
        }
        
        return false;
    }
    
}