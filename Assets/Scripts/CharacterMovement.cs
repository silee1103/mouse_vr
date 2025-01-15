using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterMovement : MonoBehaviour
{
    public float moveSpeed = 5f; // 이동 속도
    public float rotationSpeed = 720f; // 회전 속도 (초당 회전 각도)

    private Rigidbody _rb;
    private Animator _anim;

    void Start()
    {
        _rb = GetComponent<Rigidbody>();
        _anim = GetComponentInChildren<Animator>();
    }

    void Update()
    {
        // 입력 받기
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");

        // 움직임 벡터 계산
        Vector3 moveDirection = new Vector3(horizontal, 0f, vertical);

        // 움직임 적용
        if (moveDirection.magnitude > 0.1f)
        {
            // 캐릭터를 이동
            Vector3 moveVelocity = moveDirection.normalized * moveSpeed;
            _rb.velocity = new Vector3(moveVelocity.x, _rb.velocity.y, moveVelocity.z);

            // 캐릭터 회전
            Quaternion targetRotation = Quaternion.LookRotation(moveDirection);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
            _anim.SetBool("running", true);
        }
        else
        {
            // 입력이 없을 경우 속도 0
            _rb.velocity = new Vector3(0f, _rb.velocity.y, 0f);
            _anim.SetBool("running", false);
        }
    }
}
