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
    private int _maxSize = 12;
    
    public TMP_Text corridorNumberText;
    public TMP_InputField corridorNumberInput; // corridorNumber 조정용 InputField
    public Button resetButton; // 씬 리셋 버튼
    public Toggle loopToggle;
    [SerializeField] private CameraMovement cm;
    
    // Start is called before the first frame update
    void Start()
    {
        _anim = GetComponent<Animator>();
        _cameraAnim = GameObject.FindWithTag("Camera").GetComponent<Animator>();
        _platformTrigger = GameObject.Find("Animals").GetComponent<PlatformMaker>();
        
        // slider.onValueChanged.AddListener(OnSliderValueChanged);
        corridorNumberInput.onEndEdit.AddListener(OnCorridorNumberChanged);
        Invoke("SetUpTexts", 0.1f);
        loopToggle.isOn = StatusManager.sm.GetLoopFixed();
        loopToggle.onValueChanged.AddListener(StatusManager.sm.LengthFixedToggle);
    }

    void SetUpTexts()
    {
        slider.value = _maxSize / 2;
        corridorSizeText.text = (_maxSize / 2).ToString();
        
        corridorNumberText.text = _platformTrigger.corridorNumber.ToString();
    }
    
    public void OnSliderValueChanged(float value)
    {
        if (corridorSizeText != null)
        {
            corridorSizeText.text = value.ToString("F2");
            _platformTrigger.OnSliderValueChanged(value/_maxSize*2);
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
            StatusManager.sm.SetTutNum(corridorNumber);
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
        PortConnect.instance.SendResetCommand();
        PortConnect.instance.SendStartCommand();
        PortConnect.instance.TXTRANDOM = Random.Range(0, 500);
        yield return new WaitForSeconds(5.0f);
    }

    public void ResetScene()
    {
        // 현재 씬을 다시 로드
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void ResetTutorial()
    {
        StatusManager.sm.ResetTutStage();
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

    public void HeadRealism()
    {
        if (cm.enabled)
        {
            headRealismText.text = "Off";
        }
        else
        {
            headRealismText.text = "On";
        }
        cm.enabled = !cm.enabled;
    }
    
}
