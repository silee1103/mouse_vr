using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

// 튜토리얼 진행 중 플랫폼 트리거를 감지하고,
// 물과의 충돌 시 특정 동작을 수행하는 클래스
// PlatformMaker를 상속하여 기본적인 플랫폼 생성 기능 유지
public class PlatformTriggerTut : PlatformMaker
{
    private void Start()
    {
        // MovementRecorder 할당 (자식 오브젝트에서 찾기)
        mr = GetComponentInChildren<MovementRecorder>();

        platformWidth = 1; // 기본 플랫폼 너비 설정
        _spawnedPlatforms = new List<GameObject>(); // 생성된 플랫폼 리스트 초기화

        // 현재 튜토리얼 단계의 corridorNumber 가져오기
        corridorNumber = StatusManager.instance.GetTutNum();

        // 기존 섹션의 자식들을 리스트에 추가
        if (existingSectionParent != null)
        {
            foreach (Transform child in existingSectionParent.transform)
            {
                _spawnedPlatforms.Add(child.gameObject); // 기존 플랫폼 리스트에 추가
            }

            // corridorNumber에 맞게 새로운 플랫폼 추가
            AddCorrior(corridorNumber);
        }
    }

    // 물과 충돌했을 때 실행되는 코루틴 (페이드 효과 후 씬 전환)
    public override IEnumerator WaterTrigger()
    {
        yield return StartCoroutine(FadeInImage(1f)); // 화면 페이드인 효과 실행

        mr.RecordLick(); // Lick 이벤트 기록
        PortConnect.instance.SendLickCommand(); // 아두이노에 Lick 신호 전송

        yield return new WaitForSeconds(_waterOutDuration); // 물 효과 지속 시간 대기

        // 튜토리얼이 아직 남아있다면 다음 단계로 진행, 아니면 훈련 씬으로 이동
        if (StatusManager.instance.IsTutLeft())
        {
            StatusManager.instance.IncreaseTutStage(); // 튜토리얼 단계 증가
            SceneManager.LoadScene(SceneManager.GetActiveScene().name); // 현재 씬 다시 로드
        }
        else
        {
            SceneManager.LoadScene("MouseTrainScene_Corridor"); // 훈련 씬으로 이동
        }
    }
}