using UnityEngine;

// 게임의 상태를 관리하는 싱글톤 클래스
// tutNum (튜토리얼 진행 단계), corridorWidth (통로 너비), currTrainStage (현재 훈련 단계) 등의 게임 설정을 유지하고 조작하는 역할
public class StatusManager : MonoBehaviour
{
    // Singleton instance (모든 코드에서 StatusManager.instance.(public 변수나 함수 이름)으로 접근 가능)
    public static StatusManager instance;
    
    // 단계 == 통로 길이 (1마다 50cm 증가) : default로 시작, 끝 통로 2개가 존재하고 그 사이의 통로 갯수
    private int tutNum = 0;            // 현재 진행 중인 튜토리얼 단계 
    private int maxTutStage = 10;      // 최대 튜토리얼 단계
    private bool isLenFixed = false;   // 통로 길이 고정 여부
    private int currTrainStage = 0;    // 현재 훈련(Train) 단계 (Randomized) (== 통로 길이 (1마다 50cm 증가))
    
    private float corridorWidth = 12;  // 현재 설정된 통로의 너비
    private float _maxSize = 12;       // 통로의 최대 너비 값
    
    public int TXTRANDOM = 0;          // 현재 record 구별자 (0~500 사이의 랜덤한 정수 값)
    
    public int maxCorridor = 17;       // 훈련 스테이지 최대 값
    public int minCorridor = 0;        // 훈련 스테이지 최소 값
    
    private void Awake()
    {
        // Singleton 패턴 적용: 중복 인스턴스 방지
        if (instance != null && instance != this)
        {
            Destroy(gameObject); // 기존 인스턴스가 존재하면 새 인스턴스를 제거
            return;
        }
        instance = this;
        DontDestroyOnLoad(gameObject); // 씬 변경 시에도 유지
        
        // corridorWidth 초기화 (최대 사이즈 절반으로 설정)
        corridorWidth = _maxSize / 2;
        
        // TXTRANDOM 값을 0~500 사이에서 랜덤 설정
        TXTRANDOM = Random.Range(0, 500);
    }
    
    // 튜토리얼 진행 단계를 증가시키는 함수 (튜토리얼에서 통로 끝 도달 시 호출)
    public void IncreaseTutStage()
    {
        if (!isLenFixed)  // 길이가 고정되지 않은 경우에만 증가 가능
        {
            tutNum++;
        }
    }

    // 통로의 최대 너비를 반환
    public float GetCorridorMaxWidth()
    {
        return _maxSize;
    }
    
    // 현재 통로 너비를 반환
    public float GetCorridorWidth()
    {
        return corridorWidth;
    }

    // 통로 너비를 설정
    public void SetCorridorWidth(float v)
    {
        corridorWidth = v;
    }

    // 현재 튜토리얼 단계를 반환
    public int GetTutNum()
    {
        return tutNum;
    }
    
    // 튜토리얼 단계를 특정 값으로 설정
    public void SetTutNum(int i)
    {
        tutNum = i;
    }

    // train scene에서 통로 길이를 랜덤으로 변경 (train에서 통로 끝 도달 시 호출)
    public void RandomCurrTrainStage()
    {
        if (!isLenFixed)  // 길이가 고정되지 않은 경우에만 변경 가능
        {
            currTrainStage = Random.Range(minCorridor, maxCorridor);
        }
    }

    // train scene에서 현재 통로 길이를 반환
    public int GetCurrTrainStage()
    {
        return currTrainStage;
    }

    // 튜토리얼 단계를 초기화 (통로 길이를 0으로 설정)
    public void ResetTutStage()
    {
        tutNum = 0;
    }

    // 튜토리얼이 아직 남아있는지 확인
    public bool IsTutLeft()
    {
        return tutNum < maxTutStage;
    }

    // 길이 고정 여부를 설정
    public void LengthFixedToggle(bool b)
    {
        isLenFixed = b;
    }

    // 길이 고정 여부를 반환
    public bool GetLoopFixed()
    {
        return isLenFixed;
    }
}
