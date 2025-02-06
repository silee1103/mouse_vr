using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterMovement : MonoBehaviour
{
    public float speedWorldMul = 1.8f / 10f; // by z axis, in game rat size / rat real size (cm)
    public float rotationSpeed = 180f;
    
    private Animator _anim;
    
    [SerializeField]
    private CameraMovement cameraMovement; // CameraMovement 연결
    
    // 주기를 제어하기 위한 변수
    private float lastUpdateTime = 0f;
    
    private float currentSpeedZ = 0f; // 현재 속도 (전진/후진)
    private float targetSpeedZ = 0f; // 목표 속도 (전진/후진)
    
    private float currentSpeedX = 0f; // 현재 속도 (좌우 이동)
    private float targetSpeedX = 0f; // 목표 속도 (좌우 이동)
    
    private float _colliderYSize;
    
    void Start()
    {
        _anim = GetComponentInChildren<Animator>();
        _colliderYSize = GetComponent<BoxCollider>().size.y;
    }
    
    private void FixedUpdate()
    {
        // 목표 속도를 주기적으로 갱신
        if (Time.time - lastUpdateTime >= 0.1)
        {
            targetSpeedZ = Input.GetAxis("Vertical") * 50;   // 전진/후진 입력
            targetSpeedX = Input.GetAxis("Horizontal") * 50; // 좌우 이동 입력
            lastUpdateTime = Time.time; // 마지막 갱신 시간 업데이트
        }
        
        currentSpeedX = Mathf.Abs(targetSpeedX) > 1.0f ?
            Mathf.Lerp(currentSpeedX, targetSpeedX, 0.1f) : 0;
        currentSpeedZ = Mathf.Abs(targetSpeedZ) > 1.0f ?
            Mathf.Lerp(currentSpeedZ, targetSpeedZ, 0.1f) : 0;
        
        // 움직임 적용
        if (Mathf.Abs(currentSpeedZ) > 0.1f || Mathf.Abs(currentSpeedX) > 0.1f)
        {
            Vector3 moveDirection = transform.forward * (currentSpeedZ * speedWorldMul * Time.fixedDeltaTime) +
                                    transform.right * (currentSpeedX * speedWorldMul * Time.fixedDeltaTime);
            
            Vector3 lookDirection = transform.forward * (Mathf.Abs(currentSpeedZ) * speedWorldMul * Time.fixedDeltaTime) +
                                    transform.right * (currentSpeedX * speedWorldMul * Time.fixedDeltaTime);

            
            Quaternion targetRotation = Quaternion.LookRotation(lookDirection, Vector3.up);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation,
                rotationSpeed * Time.fixedDeltaTime);
       
            Debug.Log("targetSpeedX: "+ targetSpeedX+ "\nmoveDirection: " + moveDirection + "\ntargetRotation: " + targetRotation.eulerAngles);
            
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
        float normalizedSpeed = Mathf.Clamp01(Mathf.Sqrt(currentSpeedX * currentSpeedX + currentSpeedZ * currentSpeedZ) / 10f);
        cameraMovement.UpdateAnimationMode(normalizedSpeed);
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