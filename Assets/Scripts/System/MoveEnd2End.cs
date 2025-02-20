using System.Collections;
using UnityEngine;
using UnityEngine.UI;

// 캐릭터를 목표 속도로 이동시키고, WaterTrigger와 충돌 시 정지하는 클래스
// Canvas의 panel에 script를 붙이고 캐릭터를 go에, trigger 버튼을 btn에 할당하여 사용
public class MoveEnd2End : MonoBehaviour
{
    [Header("Character & Movement")]
    public GameObject go; // 이동할 캐릭터 오브젝트
    [SerializeField] private CharacterMovementHoz characterMovement; // 캐릭터의 이동을 제어하는 스크립트
    private bool isMoving = false; // 이동 중 여부 플래그
    private float fixedSpeed = 25.0f; // 목표 속도 (고정 속도로 이동)

    [Header("UI Elements")]
    [SerializeField] private Button btn; // 이동 버튼

    private void Start()
    {
        // CharacterMovementHoz 컴포넌트 가져오기
        characterMovement = go.GetComponent<CharacterMovementHoz>();
        if (characterMovement == null)
        {
            Debug.LogError("CharacterMovementHoz가 대상 객체에 없습니다!");
        }
    }

    // 버튼 클릭 시 캐릭터 이동 시작
    public void InvokeMoveEnd2End()
    {
        if (!isMoving)
        {
            btn.interactable = false; // 이동 중 버튼 비활성화
            StartCoroutine(MoveUntilWaterTrigger());
        }
    }

    // 캐릭터를 이동시키고, WaterTrigger와 충돌하면 정지하는 코루틴
    private IEnumerator MoveUntilWaterTrigger()
    {
        isMoving = true;
        characterMovement.isAuto = true; // 자동 이동 모드 활성화
        characterMovement.targetSpeed = fixedSpeed; // 고정 속도 설정

        while (isMoving)
        {
            // WaterTrigger와 충돌 검사
            if (CheckWaterTriggerCollision())
            {
                characterMovement.isAuto = false; // 자동 이동 종료
                break;
            }
            yield return null; // 다음 프레임까지 대기
        }
        
        isMoving = false;
    }

    // 캐릭터 주변에서 WaterTrigger 태그를 가진 오브젝트와 충돌 여부 검사
    private bool CheckWaterTriggerCollision()
    {
        Collider[] colliders = Physics.OverlapSphere(go.transform.position, 0.5f);
        foreach (Collider collider in colliders)
        {
            if (collider.CompareTag("WaterTrigger"))
            {
                return true; // WaterTrigger와 충돌 감지
            }
        }
        return false;
    }
}
