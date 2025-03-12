using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

// 플랫폼 트리거를 감지하고, 물과의 충돌 시 특정 동작을 수행하는 클래스
// PlatformMaker를 상속하여 기본적인 플랫폼 생성 기능을 유지하면서 추가적인 트리거 로직 구현
public class PlatformTriggerTrain : PlatformMaker
{
    private void Start()
    {
        // MovementRecorder 할당 (자식 오브젝트에서 찾기)
        mr = GetComponentInChildren<MovementRecorder>();
        
        platformWidth = 1; // 기본 플랫폼 너비 설정
        _spawnedPlatforms = new List<GameObject>(); // 생성된 플랫폼 리스트 초기화
        
        // 랜덤한 훈련 스테이지 값 설정
        StatusManager.instance.RandomCurrTrainStage();
        corridorNumber = StatusManager.instance.GetCurrTrainStage();
        
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

    // 물과 충돌했을 때 실행되는 코루틴 (페이드 효과 후 씬 리로드)
    public override IEnumerator WaterTrigger()
    {
        yield return StartCoroutine(FadeInImage(1f)); // 화면 페이드인 효과 실행

        mr.RecordLick(); // Lick 이벤트 기록
        PortConnect.instance.SendLickCommand(); // 아두이노에 Lick 신호 전송

        yield return new WaitForSeconds(_waterOutDuration); // 물 효과 지속 시간 대기
        
        // 현재 씬을 다시 로드하여 초기화
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}