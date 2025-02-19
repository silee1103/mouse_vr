using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using Random = UnityEngine.Random;

public class AdjustPanel : MonoBehaviour
{
    public Button uiPopUp;
    public TMP_Text headRealismText;
    private Animator _anim;
    private Animator _cameraAnim;
    private PlatformMaker _platformTrigger;
    
    public TMP_Text corridorSizeText;
    public Slider slider;
    public MovementRecorder mr;
    public TMP_Text corridorNumberText;
    public TMP_InputField corridorNumberInput; // corridorNumber 조정용 InputField
    public Button resetButton; // 씬 리셋 버튼
    public Toggle loopToggle;
    
    // Start is called before the first frame update
    void Start()
    {
        _anim = GetComponent<Animator>();
        _cameraAnim = GameObject.FindWithTag("Camera").GetComponent<Animator>();
        _platformTrigger = GameObject.Find("Animals").GetComponent<PlatformMaker>();
        
        // slider.onValueChanged.AddListener(OnSliderValueChanged);
        corridorNumberInput.onEndEdit.AddListener(OnCorridorNumberChanged);
        Invoke("SetUpTexts", 0.1f);
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
    }

    void SetUpTexts()
    {
        slider.value = StatusManager.instance.GetCorridorWidth();
        corridorSizeText.text = (StatusManager.instance.GetCorridorWidth()).ToString();
        
        corridorNumberText.text = _platformTrigger.corridorNumber.ToString();
        _platformTrigger.OnSliderValueChanged(StatusManager.instance.GetCorridorWidth()/StatusManager.instance.GetCorridorMaxWidth()*2);
    }
    
    public void OnSliderValueChanged(float value)
    {
        if (corridorSizeText != null)
        {
            corridorSizeText.text = value.ToString("F2");
            _platformTrigger.OnSliderValueChanged(value/StatusManager.instance.GetCorridorMaxWidth()*2);
            StatusManager.instance.SetCorridorWidth(value);
        }
    }
    
    void OnCorridorNumberChanged(string value)
    {
        if (int.TryParse(value, out int corridorNumber))
        {
            // PlatformTrigger의 corridorNumber 설정
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

    public void ResetMeasureH()
    {
        StartCoroutine(ResetMeasure());
    }
    
    
    public IEnumerator ResetMeasure()
    {
        // PortConnect.instance.SendEndCommand();
        PortConnect.instance.SendResetCommand();
        mr.SaveRemainingBuffer();
        PortConnect.instance.SendTriggerCommand();
        StatusManager.instance.TXTRANDOM = Random.Range(0, 500);
        yield return new WaitForSeconds(5.0f);
    }

    public void ResetScene()
    {
        // 현재 씬을 다시 로드
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void ResetTutorial()
    {
        StatusManager.instance.ResetTutStage();
        SceneManager.LoadScene("MouseTutorialScene_Corridor");
    }

    public void PopUpUI()
    {
        uiPopUp.interactable = false;
        if (_anim.GetBool("pop"))
        {
            _anim.SetBool("pop", false);
        }
        else
        {
            _anim.SetBool("pop", true);
        }
    }

    public void AnimationEnd()
    {
        uiPopUp.interactable = true;
    }
}
