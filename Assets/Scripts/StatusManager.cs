using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.Design.Serialization;
using UnityEngine;

public class StatusManager : MonoBehaviour
{
    public static StatusManager instance;
    private int tutNum = 0;
    private int maxTutStage = 10;
    private bool isLenFixed = false;
    private int currTrainStage = 0;
    private float corridorWidth = 12;
    private float _maxSize = 12;
    public int TXTRANDOM = 0;
    
    public int maxCorridor = 17;
    public int minCorridor = 0;
    
    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject); // 이미 존재하는 인스턴스가 있다면 새로운 객체 제거
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);
        corridorWidth = _maxSize / 2;
        TXTRANDOM = Random.Range(0, 500);
    }
    
    
    public void IncreaseTutStage()
    {
        if (!isLenFixed)
        {
            tutNum++;
        }
    }

    public float GetCorridorMaxWidth()
    {
        return _maxSize;
    }
    
    public float GetCorridorWidth()
    {
        return corridorWidth;
    }

    public void SetCorridorWidth(float v)
    {
        corridorWidth = v;
    }

    public int GetTutNum()
    {
        return tutNum;
    }
    public void SetTutNum(int i)
    {
        tutNum = i;
    }

    public void RandomCurrTrainStage()
    {
        if (!isLenFixed)
        {
            currTrainStage = Random.Range(minCorridor, maxCorridor);
        }
    }

    public int GetCurrTrainStage()
    {
        return currTrainStage;
    }

    public void ResetTutStage()
    {
        tutNum = 0;
    }

    public bool IsTutLeft()
    {
        return tutNum < maxTutStage;
    }

    public void LengthFixedToggle(bool b)
    {
        isLenFixed = b;
    }

    public bool GetLoopFixed()
    {
        return isLenFixed;
    }
}
