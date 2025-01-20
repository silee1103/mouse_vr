using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class AdjustPanel : MonoBehaviour
{
    public Button uiPopUp;
    public TMP_Text headRealismText;
    private Animator _anim;
    private Animator _cameraAnim;
    private PlatformTrigger _platformTrigger;
    
    public TMP_Text corridorSizeText;
    public Slider slider;
    [SerializeField]
    private int _maxSize = 12;
    
    // Start is called before the first frame update
    void Start()
    {
        _anim = GetComponent<Animator>();
        _cameraAnim = GameObject.FindWithTag("Camera").GetComponent<Animator>();
        _platformTrigger = GameObject.Find("Animals").GetComponent<PlatformTrigger>();
        slider.onValueChanged.AddListener(OnSliderValueChanged);
        slider.value = _maxSize/2;
    }
    
    void OnSliderValueChanged(float value)
    {
        if (corridorSizeText != null)
        {
            corridorSizeText.text = value.ToString("F2");
            _platformTrigger.OnSliderValueChanged(value/_maxSize*2);
        }
    }

    public void PopUpUI()
    {
        uiPopUp.interactable = false;
        Debug.Log(_anim.GetBool("pop"));
        if (_anim.GetBool("pop"))
        {
            Debug.Log("Set Bool pop: false");
            _anim.SetBool("pop", false);
        }
        else
        {
            Debug.Log("Set Bool pop: true");
            _anim.SetBool("pop", true);
        }
    }

    public void AnimationEnd()
    {
        uiPopUp.interactable = true;
    }

    public void HeadRealism()
    {
        if (_cameraAnim.enabled)
        {
            headRealismText.text = "Off";
        }
        else
        {
            headRealismText.text = "On";
        }
        _cameraAnim.enabled = !_cameraAnim.enabled;
    }
}
