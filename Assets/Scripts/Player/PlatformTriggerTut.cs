using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PlatformTriggerTut : PlatformMaker
{
    [SerializeField] private Image _blackImage;
    [SerializeField] private float _waterOutDuration = 5f;

    private MovementRecorder mr;

    private void Start()
    {
        mr = GetComponentInChildren<MovementRecorder>();
        platformWidth = 1;
        _spawnedPlatforms = new List<GameObject>();
        corridorNumber = StatusManager.sm.GetTutNum();
        
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

    private void OnTriggerEnter(Collider other)
    {
        if (corridorNumber < 0 && other.gameObject.CompareTag("PlatformTrigger"))
        {
            AddCorrior(-56);
        }
        if (other.gameObject.CompareTag("WaterTrigger"))
        {
            mr.RecordLick();
            StartCoroutine(WaterTrigger());
        }
        Destroy(other);
    }

    private IEnumerator WaterTrigger()
    {
        yield return StartCoroutine(FadeInImage(1f));
        PortConnect.instance.SendLickCommand();
        yield return new WaitForSeconds(_waterOutDuration);
        if (StatusManager.sm.IsTutLeft()){
            StatusManager.sm.IncreaseTutStage();
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
        else
        {
            SceneManager.LoadScene("MouseTrainScene_Corridor");
        }
    }
    
    IEnumerator FadeInImage(float duration)
    {
        Color color = _blackImage.color;
        float startAlpha = 0f;
        float endAlpha = 1f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float alpha = Mathf.Lerp(startAlpha, endAlpha, elapsed / duration);
            _blackImage.color = new Color(color.r, color.g, color.b, alpha);
            yield return null;
        }

        // Ensure final value
        _blackImage.color = new Color(color.r, color.g, color.b, endAlpha);
    }
}
