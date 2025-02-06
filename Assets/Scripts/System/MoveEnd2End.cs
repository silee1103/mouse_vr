using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngineInternal;

public class MoveEnd2End : MonoBehaviour
{
    public GameObject go;
    [SerializeField] private CharacterMovementHoz characterMovement;
    private bool isMoving = false;
    private float fixedSpeed = 25.0f; // 목표 속도
    [SerializeField] private Button btn;

    private void Start()
    {
        // CharacterMovementHoz 컴포넌트 가져오기
        characterMovement = go.GetComponent<CharacterMovementHoz>();
        if (characterMovement == null)
        {
            Debug.LogError("CharacterMovementHoz가 대상 객체에 없습니다!");
        }
    }

    public void InvokeMoveEnd2End()
    {
        if (!isMoving)
        {
            btn.interactable = false;
            StartCoroutine(MoveUntilWaterTrigger());
        }
    }

    private IEnumerator MoveUntilWaterTrigger()
    {
        isMoving = true;
        characterMovement.isAuto = true;
        characterMovement.targetSpeed = fixedSpeed;

        while (isMoving)
        {
            // WaterTrigger와 충돌 검사
            if (CheckWaterTriggerCollision())
            {
                characterMovement.isAuto = false;
                break;
            }
            yield return null;
        }
        
        isMoving = false;
    }

    private bool CheckWaterTriggerCollision()
    {
        Collider[] colliders = Physics.OverlapSphere(go.transform.position, 0.5f);
        foreach (Collider collider in colliders)
        {
            if (collider.CompareTag("WaterTrigger"))
            {
                return true;
            }
        }
        return false;
    }
}