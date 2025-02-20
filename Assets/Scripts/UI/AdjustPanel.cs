using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using Random = UnityEngine.Random;

// UI에서 조정 가능한 패널을 관리하는 클래스
// - 슬라이더 조작을 통해 통로 크기 조정 가능 : OnSliderValueChanged
// - corridorNumber를 조정하는 InputField 제공 : OnCorridorNumberChanged
// - 리셋 버튼을 통해 씬을 다시 로드 가능 : ResetScene / ResetTutorial
// - 포트 연결을 통해 아두이노 신호 조작 가능 : ResetMeasureH
public class AdjustPanel : MonoBehaviour
{
    [Header("UI Components")]
    public Button uiPopUp;              // UI 팝업 버튼
    public TMP_Text corridorSizeText;   // 통로 크기 표시 텍스트
    public Slider slider;               // 통로 크기 조정 슬라이더
    public TMP_Text corridorNumberText; // 현재 corridorNumber 표시 텍스트
    public TMP_InputField corridorNumberInput; // corridorNumber 변경용 InputField
    public Toggle loopToggle;           // 통로 길이 고정 여부를 설정하는 토글
    
    [Header("Animation Components")]
    private Animator _anim;             // AdjustPanel의 Animator
    
    [Header("Platform & Movement")]
    private PlatformMaker _platformTrigger; // PlatformMaker 참조 (플랫폼 조작)
    public MovementRecorder mr;             // MovementRecorder 참조 (데이터 저장)
    
    // Start is called before the first frame update
    void Start()
    {
        // Animator 및 관련 컴포넌트 초기화
        _anim = GetComponent<Animator>();
        _platformTrigger = GameObject.Find("Animals").GetComponent<PlatformMaker>();
        
        // UI 이벤트 리스너 등록
        corridorNumberInput.onEndEdit.AddListener(OnCorridorNumberChanged);
        loopToggle.isOn = StatusManager.instance.GetLoopFixed();
        loopToggle.onValueChanged.AddListener(StatusManager.instance.LengthFixedToggle);
        
        // 새 씬이 로드될 때 MovementRecorder 찾기
        mr = FindObjectOfType<MovementRecorder>();

        if (mr == null)
        {
            Debug.LogWarning($"[DEBUG] No MovementRecorder found in scene");
        }
        else
        {
            Debug.Log($"[DEBUG] Found MovementRecorder in scene");
        }
        
        // UI 텍스트 설정 (딜레이 후 초기화)
        Invoke("SetUpTexts", 0.1f);
    }

    // 슬라이더 및 텍스트 초기화 설정
    void SetUpTexts()
    {
        slider.value = StatusManager.instance.GetCorridorWidth();
        corridorSizeText.text = (StatusManager.instance.GetCorridorWidth()).ToString();
        
        // corridorNumber 업데이트 및 플랫폼 크기 설정
        corridorNumberText.text = _platformTrigger.corridorNumber.ToString();
        _platformTrigger.OnSliderValueChanged(StatusManager.instance.GetCorridorWidth()/StatusManager.instance.GetCorridorMaxWidth()*2);
    }
    
    // 슬라이더 값 변경 시 호출되는 함수
    public void OnSliderValueChanged(float value)
    {
        if (corridorSizeText != null)
        {
            corridorSizeText.text = value.ToString("F2");
            
            // Platform width 변경 적용
            _platformTrigger.OnSliderValueChanged(value/StatusManager.instance.GetCorridorMaxWidth()*2);
            StatusManager.instance.SetCorridorWidth(value);
        }
    }
    
    // corridorNumberInput 값 변경 시 호출되는 함수
    void OnCorridorNumberChanged(string value)
    {
        if (int.TryParse(value, out int corridorNumber))
        {
            // corridorNumber 적용 (실시간으로 갯수 바뀜)
            _platformTrigger.corridorNumber = corridorNumber;
            _platformTrigger.AddCorrior(corridorNumber);
            corridorNumberText.text = corridorNumber.ToString();
            StatusManager.instance.SetTutNum(corridorNumber);
            
            Debug.Log($"Corridor Number updated to: {corridorNumber}");
        }
        else
        {
            Debug.LogError("Invalid corridorNumber input!");
        }
    }

    // 측정 데이터 리셋 (Coroutine 실행)
    public void ResetMeasureH()
    {
        StartCoroutine(ResetMeasure());
    }
    
    // 측정 데이터를 초기화하는 코루틴
    // 이전 측정 데이터 모두 저장 + 새 seed 값 생성 + inscopics 실행
    public IEnumerator ResetMeasure()
    {
        // 측정 종료 후 리셋 명령 전송
        // PortConnect.instance.SendEndCommand();
        PortConnect.instance.SendResetCommand();
        
        // 현재 측정 중에 남아있는 record를 모두 저장
        mr.SaveRemainingBuffer();
        
        // inscopics 재실행 신호
        PortConnect.instance.SendTriggerCommand();
        
        // TXTRANDOM 값 변경 (새로운 시드 적용)
        StatusManager.instance.TXTRANDOM = Random.Range(0, 500);
        
        yield return new WaitForSeconds(5.0f);
    }
    
    // 현재 씬을 다시 로드하는 함수
    public void ResetScene()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    // 튜토리얼을 처음부터 다시 실행하는 함수
    public void ResetTutorial()
    {
        StatusManager.instance.ResetTutStage();
        SceneManager.LoadScene("MouseTutorialScene_Corridor");
    }

    
    // UI 팝업 애니메이션 실행
    public void PopUpUI()
    {
        uiPopUp.interactable = false; // 애니메이션 중 버튼 비활성화
        
        // 애니메이션 변수 조작으로 애니메이션 실행 - 현재 팝업 상태 토글
        if (_anim.GetBool("pop"))
        {
            _anim.SetBool("pop", false);
        }
        else
        {
            _anim.SetBool("pop", true);
        }
    }
    
    // 애니메이션 종료 후 UI 버튼 다시 활성화
    public void AnimationEnd()
    {
        uiPopUp.interactable = true;
    }
}
