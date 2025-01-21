using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterMovement : MonoBehaviour
{
    public float moveSpeed = 5f; // 이동 속도
    public float rotationSpeed = 720f; // 회전 속도 (초당 회전 각도)
    
    private Rigidbody _rb;
    private Animator _anim;

    [SerializeField]
    private PortConnect pm;
    
    // 주기를 제어하기 위한 변수
    private float lastUpdateTime = 0f;
    
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
    
    /*void Update()
    {
        // 현재 시간과 마지막 갱신 시간을 비교
        if (Time.time - lastUpdateTime >= pm.sendInterval)
        {
            UpdateCharacterMovement(); // 움직임 업데이트
            lastUpdateTime = Time.time; // 마지막 갱신 시간 업데이트
        }
    }

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

            // 애니메이션 실행
            _anim.SetBool("running", true);
        }
        else
        {
            // 입력이 없을 경우 속도 0
            _rb.velocity = new Vector3(0f, _rb.velocity.y, 0f);
            _anim.SetBool("running", false);
        }
    }*/
}
