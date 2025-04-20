using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterMovement : MonoBehaviour
{
    [Header("Speed Settings")]
    private float speedWorldMul = 1.08f / 12f; // 게임 내 거리 보정

    [Header("Animation & Movement")]
    private Animator _anim;
    private float lastUpdateTime = 0f;
    [SerializeField] private float currentSpeed = 0f; // 현재 이동 속도 (크기만)
    public float targetSpeed = 0f; // 목표 속도 (크기만)
    public bool isAuto = false;

    private float _colliderYSize;

    void Start()
    {
        _anim = GetComponentInChildren<Animator>();
        _colliderYSize = GetComponent<BoxCollider>().size.y;
    }

    private void FixedUpdate()
    {
        if (!isAuto)
        {
            float rawX = PortConnect.instance.speedX;
            float rawY = PortConnect.instance.speedY;

            // 속도 크기 계산
            targetSpeed = Mathf.Sqrt(rawX * rawX + rawY * rawY) * 10f; // 10배 스케일
        }

        // 현재 속도를 보간
        currentSpeed = Mathf.Lerp(currentSpeed, targetSpeed, 0.8f);

        // 회전 방향 가져오기
        float rot = PortConnect.instance.headRotation; // degrees
        Vector3 moveDir = Quaternion.Euler(0f, rot, 0f) * Vector3.forward; // 회전 적용된 방향
        Vector3 moveVector = moveDir.normalized * currentSpeed * speedWorldMul * Time.fixedDeltaTime;

        if (moveVector.magnitude > 0.01f)
        {
            if (CheckHitWall(moveVector))
                moveVector = Vector3.zero;

            transform.Translate(moveVector, Space.World);

            // 캐릭터가 바라보는 방향도 회전시키기 (부드럽게)
            Quaternion targetRotation = Quaternion.LookRotation(moveDir);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, 0.2f);

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
        float detectionRange = 1f;

        Vector3 rayOrigin = transform.position + Vector3.up * (_colliderYSize * 0.5f);

        if (Physics.Raycast(rayOrigin, movement, out RaycastHit hit, detectionRange))
        {
            if (hit.collider.CompareTag("Wall"))
                return true;
        }

        return false;
    }
}
