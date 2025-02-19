using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlatformTriggerTut : PlatformMaker
{
    private void Start()
    {
        mr = GetComponentInChildren<MovementRecorder>();
        platformWidth = 1;
        _spawnedPlatforms = new List<GameObject>();
        corridorNumber = StatusManager.instance.GetTutNum();
        
        // 기존 섹션의 자식들을 리스트로 추가
        if (existingSectionParent != null)
        {
            // 부모의 자식 객체들을 가져와 리스트에 추가
            foreach (Transform child in existingSectionParent.transform)
            {
                _spawnedPlatforms.Add(child.gameObject);
            }
            
            AddCorrior(corridorNumber);
        }
    }

    public override IEnumerator WaterTrigger()
    {
        yield return StartCoroutine(FadeInImage(1f));
        mr.RecordLick();
        PortConnect.instance.SendLickCommand();
        yield return new WaitForSeconds(_waterOutDuration);
        if (StatusManager.instance.IsTutLeft()){
            StatusManager.instance.IncreaseTutStage();
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
        else
        {
            SceneManager.LoadScene("MouseTrainScene_Corridor");
        }
    }
    
}
